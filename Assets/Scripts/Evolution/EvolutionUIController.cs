using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BotVsDungeon.Evolution
{
    public class EvolutionUIController : MonoBehaviour
    {
        [SerializeField] private EvolutionManager evolutionManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Dropdown goalDropdown;
        [SerializeField] private Slider populationSizeSlider;
        [SerializeField] private Slider mutationRateSlider;
        [SerializeField] private Slider recombinationRateSlider;
        [SerializeField] private TMP_Dropdown speedDropdown;
        [SerializeField] private TMP_Text generationText;
        [SerializeField] private TMP_Text bestFitnessText;
        [SerializeField] private TMP_Text averageFitnessText;
        [SerializeField] private TMP_Text bestPreviewText;

        private void Awake()
        {
            SetupDropdowns();
            if (evolutionManager != null)
            {
                evolutionManager.OnGenerationCompleted += OnGenerationCompleted;
            }

            SyncControlsFromSettings();
            RefreshDisplay(evolutionManager?.LastReport);
        }

        private void OnDestroy()
        {
            if (evolutionManager != null)
            {
                evolutionManager.OnGenerationCompleted -= OnGenerationCompleted;
            }
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }
        }

        public void OnStartPressed()
        {
            evolutionManager?.StartEvolution();
        }

        public void OnStopPressed()
        {
            evolutionManager?.StopEvolution();
        }

        public void OnViewBestPressed()
        {
            evolutionManager?.LoadBestDungeon();
        }

        public void OnViewGenerationBestPressed()
        {
            evolutionManager?.ViewCurrentGenerationBest();
        }

        public void OnSaveBestPressed()
        {
            if (evolutionManager != null && evolutionManager.SaveBestDungeon("EvolutionLab_Best", out string message))
            {
                Debug.Log(message);
            }
        }

        public void OnGoalChanged(int value)
        {
            if (evolutionManager == null) return;
            EvolutionGoal goal = (EvolutionGoal)Mathf.Clamp(value, 0, System.Enum.GetValues(typeof(EvolutionGoal)).Length - 1);
            EvolutionSettings preset = EvolutionGoalPresets.BuildPreset(goal);
            ApplyPresetToSettings(preset);
            SyncControlsFromSettings();
        }

        public void OnPopulationChanged(float value)
        {
            if (evolutionManager == null) return;
            evolutionManager.Settings.populationSize = Mathf.RoundToInt(value);
        }

        public void OnMutationChanged(float value)
        {
            if (evolutionManager == null) return;
            evolutionManager.Settings.mutationProbability = value;
        }

        public void OnRecombinationChanged(float value)
        {
            if (evolutionManager == null) return;
            evolutionManager.Settings.recombinationProbability = value;
        }

        public void OnSpeedChanged(int value)
        {
            if (evolutionManager == null) return;
            evolutionManager.Settings.simulationSpeed = (EvolutionSpeed)Mathf.Clamp(value, 0, 2);
        }

        private void SetupDropdowns()
        {
            if (goalDropdown != null && goalDropdown.options.Count == 0)
            {
                goalDropdown.AddOptions(new List<string>
                {
                    EvolutionGoal.Brutal.ToString(),
                    EvolutionGoal.Fair.ToString(),
                    EvolutionGoal.Puzzle.ToString(),
                    EvolutionGoal.Balanced.ToString(),
                    EvolutionGoal.StressTest.ToString()
                });
            }

            if (speedDropdown != null && speedDropdown.options.Count == 0)
            {
                speedDropdown.AddOptions(new List<string>
                {
                    EvolutionSpeed.Normal.ToString(),
                    EvolutionSpeed.Fast.ToString(),
                    EvolutionSpeed.VeryFast.ToString()
                });
            }
        }

        private void SyncControlsFromSettings()
        {
            if (evolutionManager == null)
            {
                return;
            }

            EvolutionSettings settings = evolutionManager.Settings;
            if (goalDropdown != null) goalDropdown.value = (int)settings.goal;
            if (populationSizeSlider != null) populationSizeSlider.SetValueWithoutNotify(settings.populationSize);
            if (mutationRateSlider != null) mutationRateSlider.SetValueWithoutNotify(settings.mutationProbability);
            if (recombinationRateSlider != null) recombinationRateSlider.SetValueWithoutNotify(settings.recombinationProbability);
            if (speedDropdown != null) speedDropdown.value = (int)settings.simulationSpeed;
        }

        private void ApplyPresetToSettings(EvolutionSettings preset)
        {
            if (evolutionManager == null || preset == null)
            {
                return;
            }

            EvolutionSettings settings = evolutionManager.Settings;
            settings.goal = preset.goal;
            settings.populationSize = preset.populationSize;
            settings.maxGenerations = preset.maxGenerations;
            settings.mutationProbability = preset.mutationProbability;
            settings.recombinationProbability = preset.recombinationProbability;
            settings.survivorRatio = preset.survivorRatio;
            settings.simulationDepthSeconds = preset.simulationDepthSeconds;
            settings.botCount = preset.botCount;
            settings.simulationSpeed = preset.simulationSpeed;
            settings.customWeights = preset.customWeights;
        }

        private void OnGenerationCompleted(EvolutionGenerationReport report)
        {
            RefreshDisplay(report);
        }

        private void RefreshDisplay(EvolutionGenerationReport report)
        {
            if (generationText != null)
            {
                generationText.text = report == null ? "Generation: -" : $"Generation: {report.generationNumber}";
            }

            if (bestFitnessText != null)
            {
                bestFitnessText.text = report == null ? "Best Fitness: -" : $"Best Fitness: {report.bestFitness:0.00}";
            }

            if (averageFitnessText != null)
            {
                averageFitnessText.text = report == null ? "Average Fitness: -" : $"Average Fitness: {report.averageFitness:0.00}";
            }

            if (bestPreviewText != null)
            {
                bestPreviewText.text = report == null
                    ? "No evolved dungeon yet."
                    : $"Goal: {report.goal}\nSurvival: {report.bestSurvivalRate:P0}\nPopulation: {report.populationSize}";
            }
        }
    }
}
