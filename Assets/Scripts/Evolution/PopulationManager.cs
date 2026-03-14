using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BotVsDungeon.Evolution
{
    public class PopulationManager : MonoBehaviour
    {
        public List<EvolutionCandidate> SortByFitness(List<EvolutionCandidate> population)
        {
            return population.OrderByDescending(c => c.fitness).ToList();
        }

        public List<EvolutionCandidate> SelectSurvivors(List<EvolutionCandidate> sortedPopulation, float survivorRatio)
        {
            if (sortedPopulation == null || sortedPopulation.Count == 0)
            {
                return new List<EvolutionCandidate>();
            }

            int survivorCount = Mathf.Clamp(Mathf.CeilToInt(sortedPopulation.Count * survivorRatio), 1, sortedPopulation.Count);
            return sortedPopulation.Take(survivorCount).ToList();
        }

        public float AverageFitness(List<EvolutionCandidate> population)
        {
            if (population == null || population.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            foreach (EvolutionCandidate candidate in population)
            {
                total += candidate.fitness;
            }

            return total / population.Count;
        }
    }
}
