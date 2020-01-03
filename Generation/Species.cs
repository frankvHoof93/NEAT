using nl.FvH.NEAT.Brain;
using nl.FvH.NEAT.Util;
using System;
using System.Collections.Generic;

namespace nl.FvH.NEAT.Generation
{
    internal class Species
    {
        #region Variables
        #region Internal
        /// <summary>
        /// Amount of Genomes in Species
        /// </summary>
        internal uint Count => (uint)genomes.Count;
        /// <summary>
        /// Mascot for Species
        /// </summary>
        internal Genome Mascot { get; private set; }
        /// <summary>
        /// ReadOnly-List of Genomes in Species
        /// </summary>
        internal IReadOnlyList<Genome> Genomes => genomes.AsReadOnly();
        /// <summary>
        /// Indexer for Genomes in this Species
        /// </summary>
        /// <param name="index">Index in Genome-List</param>
        /// <returns>Genome in Species for Index</returns>
        internal Genome this[int index] { get { return genomes[index]; } private set { genomes[index] = value; } }
        #endregion

        #region Private
        /// <summary>
        /// Genomes in Species
        /// </summary>
        private readonly List<Genome> genomes = new List<Genome>();
        #endregion
        #endregion

        #region Methods
        #region Constructors
        /// <summary>
        /// Creates a Species with a Mascot
        /// </summary>
        /// <param name="mascot">Mascot for Species</param>
        internal Species(Genome mascot)
        {
            Mascot = mascot;
        }
        #endregion

        #region Getters
        /// <summary>
        /// Gets Fitness for Species (Avg of Genomes)
        /// </summary>
        /// <returns>Average Fitness of Genomes in Species</returns>
        internal float GetAvgFitness()
        {
            return GetFitness() / Count;
        }
        /// <summary>
        /// Gets Fitness for Species (Sum of Genomes)
        /// </summary>
        /// <returns>Sum of Fitness for Genomes in Species</returns>
        internal float GetFitness()
        {
            float sum = 0;
            for (int i = 0; i < Count; i++)
                sum += this[i].Fitness;
            return sum;
        }
        #endregion

        #region Genomes
        /// <summary>
        /// Adds a Genome to this Species (if it is not in the species yet)
        /// </summary>
        /// <param name="genome">Genome to Add</param>
        internal void AddGenome(Genome genome)
        {
            if (!genomes.Contains(genome))
                genomes.Add(genome);
        }
        /// <summary>
        /// Clears Genomes, but attempts to set new Mascot first 
        /// (to ensure an up-to-date Mascot from only 1 generation ago)
        /// </summary>
        internal void Clear()
        {
            if (Count > 0)
                Mascot = Functions.GetRandomElement(genomes);
            genomes.Clear();
        }
        #endregion

        #region Breeding
        /// <summary>
        /// Kills off percentage of genomes before breeding
        /// </summary>
        /// <param name="live">Percentage that lives (0-100)</param>
        internal void KillOffPercentage(float live)
        {
            if (live < 0 || live > 100)
                throw new ArgumentException("Invalid percentage", "live");
            genomes.Sort(Comparer<Genome>.Create((g1, g2) => -1 * g1.Fitness.CompareTo(g2.Fitness))); // Sort DESCENDING Based on Fitness
            int newPopSize = (int)Math.Ceiling(live * Count);
            genomes.RemoveRange(newPopSize, (int)Count - newPopSize);
        }
        /// <summary>
        /// Breeds new population within species
        /// </summary>
        /// <param name="popSize">Size for new population</param>
        internal void BreedPopulation(int popSize, float mutationChance)
        {
            if (Count < 2)
                throw new InvalidOperationException("Not enough parents to breed");
            List<Genome> parents = new List<Genome>(genomes);
            genomes.Clear();
            for (int i = 0; i < popSize; i++)
            {
                int p1 = Functions.GetRandomNumber(0, parents.Count); // Find Random Parents
                int p2 = Functions.GetRandomNumber(0, parents.Count);
                if (p1 == p2)
                {
                    i--;
                    continue;
                }
                Genome child = Genome.Breed(parents[p1], parents[p2]); // Breed
                genomes.Add(child); // Add to population
                if (Functions.GetRandomNumber(0f, 100f) < mutationChance)
                    child.Mutate(); // Mutate
            }
        }
        #endregion
        #endregion
    }
}