using nl.FvH.NEAT.Brain;
using nl.FvH.NEAT.Brain.Genes;
using nl.FvH.NEAT.Enums;
using nl.FvH.NEAT.Factories;
using nl.FvH.NEAT.Generation;
using nl.FvH.NEAT.Util;
using System;
using System.Collections.Generic;

namespace nl.FvH.NEAT
{
    public class NEATController
    {
        #region Variables
        #region Public
        /// <summary>
        /// Size of Population (Number of Genomes)
        /// </summary>
        public uint PopulationSize { get; private set; }
        /// <summary>
        /// Current Population of Genomes
        /// </summary>
        public IReadOnlyList<Genome> Population => population.AsReadOnly();
        #endregion

        #region NetworkIO
        /// <summary>
        /// Number of Input-Nodes per Genome
        /// </summary>
        private readonly uint inputNodes;
        /// <summary>
        /// Number of Output-Nodes per Genome
        /// </summary>
        private readonly uint outputNodes;
        #endregion

        #region Breeding
        /// <summary>
        /// Percentage of Genomes within each species that are allowed to breed
        /// </summary>
        private float breedingPercentage;
        /// <summary>
        /// Threshold for Compatibility between Genomes
        /// </summary>
        private float compatibilityThreshold;
        /// <summary>
        /// Chance for Mutation (Percentage) of each Genome when Breeding
        /// </summary>
        private float mutationChance;
        /// <summary>
        /// Constant used for calculating CompatibilityDistance
        /// </summary>
        private float c1, c2, c3;
        #endregion

        #region Population
        /// <summary>
        /// Current Population of Genomes
        /// </summary>
        private readonly List<Genome> population = new List<Genome>();
        /// <summary>
        /// Species of Genomes (for 'previous' generation)
        /// </summary>
        private readonly List<Species> species = new List<Species>();
        #endregion
        #endregion

        #region Methods
        #region Constructors
        /// <summary>
        /// Constructor for a NEATController
        /// </summary>
        /// <param name="popSize">Size of Population (Number of Genomes in a Generation)</param>
        /// <param name="numInputs">Number of Input-Nodes per Genome</param>
        /// <param name="numOutputs">Number of Output-Nodes per Genome</param>
        /// <param name="mutation">Chance (0-100) for Mutation of each Genome when Breeding</param>
        /// <param name="breed">Percentage (0-100) of Genomes within each species that are allowed to breed</param>
        /// <param name="compThresh">Threshold for Compatibility between Genomes</param>
        /// <param name="c1">Constant used for calculating CompatibilityDistance</param>
        /// <param name="c2">Constant used for calculating CompatibilityDistance</param>
        /// <param name="c3">Constant used for calculating CompatibilityDistance</param>
        public NEATController(uint popSize, uint numInputs, uint numOutputs, float mutation, float breed, float compThresh, float c1, float c2, float c3)
        {
            if (popSize == 0)
                throw new ArgumentNullException("popSize", "Population-Size cannot be 0");
            if (numInputs == 0)
                throw new ArgumentNullException("numInputs", "Number of Inputs cannot be 0");
            if (numOutputs == 0)
                throw new ArgumentNullException("numOutputs", "Number of Outputs cannot be 0");
            if (mutation < 0 || mutation > 100)
                throw new ArgumentOutOfRangeException("mutation", "Mutation-Chance must be between 0 and 100 percent");
            if (breed < 0 || breed > 100)
                throw new ArgumentOutOfRangeException("breed", "Breeding-Amount must be between 0 and 100 percent");
            PopulationSize = popSize;
            inputNodes = numInputs;
            outputNodes = numOutputs;
            mutationChance = mutation;
            breedingPercentage = breed;
            compatibilityThreshold = compThresh;
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
        }
        #endregion

        #region Public
        /// <summary>
        /// Breeds a Population of Genomes
        /// </summary>
        public void BreedNewPopulation()
        {
            if (population.Count == 0)
            {
                CreateInitialPopulation();
                return;
            }
            if (species.Count == 0) // No Species from previous Generation
                Functions.FisherYatesShuffle(population); // Shuffle Previous Generation (so that initial Species-Mascots are randomized)
            // Clear old population to make room for new population (references exist through species)
            population.Clear();
            // Create new Population
            float sum = 0;
            // Find population-size per species
            for (int i = 0; i < species.Count; i++)
                sum += species[i].GetFitness(); // Total Fitness
            for (int i = 0; i < species.Count; i++) // For Each Species
            {
                Species s = species[i];
                int popSize = (int)Math.Ceiling(s.GetFitness() / sum * PopulationSize);
                // Darwin multikill:
                s.KillOffPercentage(breedingPercentage);
                // Breed within Species
                s.BreedPopulation(popSize, mutationChance);
                // Add to population
                population.AddRange(s.Genomes);
            }
            while (population.Count > PopulationSize) // Remove Excess from Math.Ceil
                population.Remove(Functions.GetRandomElement(population)); // Darwin Sniper
            MutationFactory.EndGeneration();
        }

        #region SetVariables
        /// <summary>
        /// Sets new PopulationSize
        /// </summary>
        /// <param name="newSize">Size to set (> 0)</param>
        /// <param name="breedNewPopulation">Whether to breed a new population with new size</param>
        public void SetPopulationSize(uint newSize, bool breedNewPopulation = false)
        {
            if (newSize == 0)
                throw new ArgumentNullException("newSize", "PopulationSize cannot be 0");
            PopulationSize = newSize;
            if (breedNewPopulation)
                BreedNewPopulation();
        }
        /// <summary>
        /// Sets percentage of Population allowed to Breed
        /// </summary>
        /// <param name="newPercentage">Percentage to set (0-100)</param>
        public void SetBreedingPercentage(float newPercentage)
        {
            if (newPercentage < 0 || newPercentage > 100)
                throw new ArgumentOutOfRangeException("newPercentage", "Breeding-Amount must be between 0 and 100 percent");
            breedingPercentage = newPercentage;
        }
        /// <summary>
        /// Sets variables for calculating compatibility
        /// </summary>
        /// <param name="threshold">Threshold for Compatibility</param>
        /// <param name="c1">Constant for calculating compatibility</param>
        /// <param name="c2">Constant for calculating compatibility</param>
        /// <param name="c3">Constant for calculating compatibility</param>
        public void SetCompatibility(float threshold, float c1, float c2, float c3)
        {
            compatibilityThreshold = threshold;
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
        }
        /// <summary>
        /// Sets mutation-chance used after breeding
        /// </summary>
        /// <param name="mutate">Percentage-chance for mutation (0-100)</param>
        public void SetMutationChance(float mutate)
        {
            if (mutate < 0 || mutate > 100)
                throw new ArgumentOutOfRangeException("mutation", "Mutation-Chance must be between 0 and 100 percent");
            mutationChance = mutate;
        }
        #endregion
        #endregion

        #region Private
        /// <summary>
        /// Creates initial Genome-Population
        /// </summary>
        private void CreateInitialPopulation()
        {
            // This list is eventually used to store all nodes, thus the large capacity is set
            List<NodeGene> input = new List<NodeGene>((int)(inputNodes + outputNodes));
            // Create Input-Nodes
            for (int i = 0; i < inputNodes; i++)
                input.Add(new NodeGene(NodeType.INPUT));
            // This list is only used to temporarily store the output-nodes
            List<NodeGene> output = new List<NodeGene>((int)outputNodes);
            // Create Output-Nodes
            for (int i = 0; i < outputNodes; i++)
                output.Add(new NodeGene(NodeType.OUTPUT));
            // Create Connections between each input- and output-node
            List<ConnectionGene> connections = new List<ConnectionGene>();
            for (int i = 0; i < input.Count; i++)
            {
                NodeGene inputNode = input[i];
                for (int j = 0; j < output.Count; j++)
                    connections.Add(new ConnectionGene(inputNode, output[i], Functions.GetRandomNumber(-1f, 1f + float.Epsilon)));
            }
            // Add Output-Nodes to Input-List to create a single list of NodeGenes
            input.AddRange(output);
            // Create Genomes
            for (int i = 0; i < PopulationSize; i++)
            {
                Genome g = new Genome(input, connections);
                g.Mutate(); // Mutate all genomes to ensure they are not all the same
                population.Add(g);
            }
        }

        /// <summary>
        /// Pushes Genomes into Species based on Compatibility-Distance
        /// </summary>
        private void Speciate()
        {
            // Each existing species is represented by a random genome inside
            // the species from the previous generation.
            // 
            // A given genome g in the current generation is placed in the first species 
            // in which g is compatible with the representative genome of that species. 
            // This way, species do not overlap.
            //
            // If g is not compatible with any existing species, 
            // a new species is created with g as its representative.
            for (int i = 0; i < species.Count; i++)
                species[i].Clear(); // Clear previous Species and set new Mascots from previous Generation
            for (int i = 0; i < population.Count; i++) // For each Genome in Population
            {
                Genome g = population[i];
                bool addedToSpecies = false;
                for (int j = 0; j < species.Count; j++) // Loop through Species (in order) to find first closest match
                {
                    Species s = species[j];
                    if (Functions.CompatibilityDistance(g, s.Mascot, c1, c2, c3) < compatibilityThreshold)
                    {
                        s.AddGenome(g); // Add to Speciest
                        addedToSpecies = true;
                        break; // Break from Species-Loop
                    }
                }
                if (!addedToSpecies) // Create new Species with g as Mascot
                {
                    Species s = new Species(g);
                    s.AddGenome(g); // Add g as member
                    species.Add(s);
                }
            }
            // FitnessSharing: f'(i) = f(i) / Species.Count (reduced calculation due to speciation)
            for (int i = 0; i < species.Count; i++) // For Each Species
            {
                Species species1 = species[i];
                if (species1.Count > 1) // If Species has more than 1 member
                    for (int j = 0; j < species1.Genomes.Count; j++) // For Each Genomes
                    {
                        Genome g = species1[j];
                        g.SetFitness(g.Fitness / species1.Count); // Update Fitness
                    }
            }
        }
        #endregion
        #endregion
    }
}
