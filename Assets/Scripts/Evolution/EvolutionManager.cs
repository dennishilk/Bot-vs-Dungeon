using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public class EvolutionManager : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private DungeonGenerator dungeonGenerator;
        [SerializeField] private DungeonSerializer dungeonSerializer;
        [SerializeField] private DungeonSaveManager dungeonSaveManager;
        [SerializeField] private ShareCodeManager shareCodeManager;
        [SerializeField] private EvolutionEvaluator evaluator;
        [SerializeField] private PopulationManager populationManager;
        [SerializeField] private MutationEngine mutationEngine;
        [SerializeField] private RecombinationEngine recombinationEngine;

        [Header("Experimental Settings")]
        [SerializeField] private EvolutionSettings settings = new();
        [SerializeField] private float targetFitnessThreshold = 1.2f;

        [Header("Optional Live Output")]
        [SerializeField] private TMP_Text stateText;

        private Coroutine _loopRoutine;
        private System.Random _rng;

        public int CurrentGeneration { get; private set; }
        public EvolutionCandidate BestEverCandidate { get; private set; }
        public EvolutionGenerationReport LastReport { get; private set; }
        public bool IsRunning => _loopRoutine != null;
        public EvolutionSettings Settings => settings;

        public event Action<EvolutionGenerationReport> OnGenerationCompleted;

        public void StartEvolution()
        {
            if (IsRunning)
            {
                return;
            }

            _rng = settings.deterministic ? new System.Random(settings.deterministicSeed) : new System.Random(Environment.TickCount);
            _loopRoutine = StartCoroutine(EvolutionLoop());
        }

        public void StopEvolution()
        {
            if (!IsRunning)
            {
                return;
            }

            StopCoroutine(_loopRoutine);
            _loopRoutine = null;
            Time.timeScale = 1f;
            SetStatus("Evolution stopped.");
        }

        public void LoadBestDungeon()
        {
            if (BestEverCandidate == null)
            {
                return;
            }

            dungeonSerializer?.ApplyLayout(BestEverCandidate.genome.ToSaveData("Evolution_Best_Loaded"), out _);
        }

        public bool SaveBestDungeon(string name, out string message)
        {
            message = "No evolved dungeon available.";
            if (BestEverCandidate == null || dungeonSaveManager == null)
            {
                return false;
            }

            string saveName = string.IsNullOrWhiteSpace(name) ? $"Evolution_{settings.goal}_{DateTime.UtcNow:yyyyMMdd_HHmmss}" : name;
            return dungeonSaveManager.SaveImportedLayout(saveName, BestEverCandidate.genome.ToSaveData(saveName), true, out message);
        }

        public bool ExportBestShareCode(out string shareCode)
        {
            shareCode = string.Empty;
            if (BestEverCandidate == null || shareCodeManager == null || dungeonSerializer == null)
            {
                return false;
            }

            dungeonSerializer.ApplyLayout(BestEverCandidate.genome.ToSaveData("Evolution_Best_Export"), out _);
            shareCode = shareCodeManager.ExportCurrentDungeonCode("Evolution_Best");
            return !string.IsNullOrWhiteSpace(shareCode);
        }

        public void ViewCurrentGenerationBest()
        {
            if (LastReport?.bestCandidate == null)
            {
                return;
            }

            dungeonSerializer?.ApplyLayout(LastReport.bestCandidate.genome.ToSaveData("Evolution_CurrentBest"), out _);
        }

        private IEnumerator EvolutionLoop()
        {
            ApplySimulationSpeed();
            List<DungeonGenome> genomes = GenerateInitialPopulation();
            CurrentGeneration = 0;
            BestEverCandidate = null;

            while (CurrentGeneration < settings.maxGenerations)
            {
                CurrentGeneration++;
                List<EvolutionCandidate> evaluated = new();

                foreach (DungeonGenome genome in genomes)
                {
                    EvolutionMetrics metrics = null;
                    float fitness = 0f;
                    yield return evaluator.EvaluateRoutine(genome, settings.goal, settings, (m, f) =>
                    {
                        metrics = m;
                        fitness = f;
                    });

                    evaluated.Add(new EvolutionCandidate { genome = genome, metrics = metrics ?? new EvolutionMetrics(), fitness = fitness });
                    if (settings.skipBotVisuals)
                    {
                        yield return null;
                    }
                }

                List<EvolutionCandidate> sorted = populationManager.SortByFitness(evaluated);
                EvolutionCandidate best = sorted[0];
                if (BestEverCandidate == null || best.fitness > BestEverCandidate.fitness)
                {
                    BestEverCandidate = best;
                }

                LastReport = new EvolutionGenerationReport
                {
                    generationNumber = CurrentGeneration,
                    populationSize = sorted.Count,
                    bestFitness = best.fitness,
                    averageFitness = populationManager.AverageFitness(sorted),
                    bestSurvivalRate = best.metrics.survivalRate,
                    goal = settings.goal,
                    bestCandidate = best
                };

                OnGenerationCompleted?.Invoke(LastReport);
                SetStatus($"Evolution G{CurrentGeneration}: best {best.fitness:0.00} / avg {LastReport.averageFitness:0.00}");

                if (best.fitness >= targetFitnessThreshold)
                {
                    break;
                }

                List<EvolutionCandidate> survivors = populationManager.SelectSurvivors(sorted, settings.survivorRatio);
                genomes = BreedNextGeneration(survivors, settings.populationSize);
                yield return null;
            }

            _loopRoutine = null;
            Time.timeScale = 1f;
            SetStatus($"Evolution complete. Best fitness: {BestEverCandidate?.fitness ?? 0f:0.00}");
        }

        private List<DungeonGenome> GenerateInitialPopulation()
        {
            List<DungeonGenome> genomes = new();
            DirectorGoal mappedGoal = settings.goal switch
            {
                EvolutionGoal.Brutal => DirectorGoal.Brutal,
                EvolutionGoal.Fair => DirectorGoal.Fair,
                EvolutionGoal.Puzzle => DirectorGoal.Puzzle,
                EvolutionGoal.StressTest => DirectorGoal.StressTest,
                _ => DirectorGoal.Balanced
            };

            for (int i = 0; i < settings.populationSize; i++)
            {
                DirectorParameters parameters = dungeonGenerator.DefaultParameters;
                GeneratedDungeonLayout layout = dungeonGenerator.GenerateLayout(mappedGoal, parameters, i);
                genomes.Add(DungeonGenome.FromLayout(layout));
            }

            return genomes;
        }

        private List<DungeonGenome> BreedNextGeneration(List<EvolutionCandidate> survivors, int populationSize)
        {
            List<DungeonGenome> next = new();
            if (survivors.Count == 0)
            {
                return GenerateInitialPopulation();
            }

            foreach (EvolutionCandidate survivor in survivors)
            {
                next.Add(survivor.genome.Clone());
            }

            while (next.Count < populationSize)
            {
                DungeonGenome parentA = survivors[_rng.Next(survivors.Count)].genome;
                DungeonGenome child = parentA.Clone();

                if (survivors.Count > 1 && _rng.NextDouble() < settings.recombinationProbability)
                {
                    DungeonGenome parentB = survivors[_rng.Next(survivors.Count)].genome;
                    child = recombinationEngine.Recombine(parentA, parentB, _rng);
                }

                child = mutationEngine.Mutate(child, settings.mutationProbability, _rng);
                next.Add(child);
            }

            return next;
        }

        private void ApplySimulationSpeed()
        {
            Time.timeScale = settings.simulationSpeed switch
            {
                EvolutionSpeed.Fast => 2f,
                EvolutionSpeed.VeryFast => 4f,
                _ => 1f
            };
        }

        private void SetStatus(string text)
        {
            if (stateText != null)
            {
                stateText.text = text;
            }

            Debug.Log($"[EvolutionLab] {text}");
        }
    }
}
