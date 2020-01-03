using nl.FvH.NEAT.Brain;
using nl.FvH.NEAT.Brain.Genes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace nl.FvH.NEAT.Util
{
    internal static class Functions
    {
        #region Variables
        /// <summary>
        /// Random Number Generator used in functions
        /// </summary>
        private static Random r = new Random();
        #endregion

        #region Methods
        #region RandomNumber
        /// <summary>
        /// Gets a Random Number (Using System.Random)
        /// </summary>
        /// <param name="minimum">INCLUSIVE</param>
        /// <param name="maximum">EXCLUSIVE</param>
        /// <returns>Random number between minimum and maximum</returns>
        internal static float GetRandomNumber(float minimum, float maximum)
        {
            return (float)(r.NextDouble() * (maximum - minimum) + minimum);
        }
        /// <summary>
        /// Gets a Random Number (Using System.Random)
        /// </summary>
        /// <param name="minimum">INCLUSIVE</param>
        /// <param name="maximum">EXCLUSIVE</param>
        /// <returns>Random number between minimum and maximum</returns>
        internal static int GetRandomNumber(int minimum, int maximum)
        {
            return r.Next(minimum, maximum);
        }
        #endregion

        #region RandomElement
        /// <summary>
        /// Returns a random element from a list (or a default value if the list has no elements)
        /// </summary>
        /// <typeparam name="T">Type of elements in list</typeparam>
        /// <param name="list">List to get element from</param>
        /// <returns>Random element from list (or default value if the list has no elements)</returns>
        public static T GetRandomElement<T>(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            // Get the number of elements in the collection
            int count = list.Count();
            // If there are no elements in the collection, return the default value of T
            if (count == 0)
                return default;
            // Get a random index
            int index = r.Next(list.Count());
            // When the collection has 100 elements or less, get the random element
            // by traversing the collection one element at a time.
            if (count <= 100)
            {
                using (IEnumerator<T> enumerator = list.GetEnumerator())
                {
                    // Move down the collection one element at a time.
                    // When index is -1 we are at the random element location
                    while (index >= 0 && enumerator.MoveNext())
                        index--;
                    // Return the current element
                    return enumerator.Current;
                }
            }
            // Get an element using LINQ which casts the collection
            // to an IList and indexes into it.
            return list.ElementAt(index);
        }
        #endregion

        #region ShuffleElements
        /// <summary>
        /// Shuffles Elements in a List using Fisher-Yates Shuffle
        /// </summary>
        /// <typeparam name="T">Type of Element in List</typeparam>
        /// <param name="list">List to Shuffle</param>
        public static void FisherYatesShuffle<T>(IList<T> list)
        {
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                int n = list.Count;
                while (n > 1)
                {
                    byte[] box = new byte[1];
                    provider.GetBytes(box);
                    while (!(box[0] < n * (byte.MaxValue / n)))
                        provider.GetBytes(box);
                    int k = box[0] % n;
                    n--;
                    T temp = list[k];
                    list[k] = list[n];
                    list[n] = temp;
                }
            }
        }
        #endregion

        #region Genomes
        /// <summary>
        /// Checks whether a Genome has a Connection for a set of Nodes
        /// </summary>
        /// <param name="genome">Genome to check</param>
        /// <param name="inputNode">InputNode for Connection</param>
        /// <param name="outputNode">OutputNode for Connection</param>
        /// <returns>True if Connection Exists</returns>
        internal static bool GenomeHasConnection(Genome genome, NodeGene inputNode, NodeGene outputNode)
        {
            foreach (ConnectionGene conn in genome.Connections.Values)
                if (conn.In.Equals(inputNode) && conn.Out.Equals(outputNode))
                    return true;
            return false;
        }
        /// <summary>
        /// Calculates Compatibility-Distance between two Genomes
        /// </summary>
        /// <param name="g1">Genome 1</param>
        /// <param name="g2">Genome 2</param>
        /// <param name="c1">Constant 1</param>
        /// <param name="c2">Constant 2</param>
        /// <param name="c3">Constant 3</param>
        /// <returns>Compatibility-Distance between two Genomes</returns>
        internal static float CompatibilityDistance(Genome g1, Genome g2, float c1, float c2, float c3)
        {
            // Make sure G1 is the larger genome
            if (g1.NumGenes < g2.NumGenes)
            {
                Genome temp = g1;
                g1 = g2;
                g2 = temp;
            }
            // N = Amount of genes (connections) in larger genome             
            uint N = g1.NumGenes;
            // Find all matching & non-matching Connections via Innovation-Numbers
            List<Tuple<ConnectionGene, ConnectionGene>> matching = new List<Tuple<ConnectionGene, ConnectionGene>>();
            List<ulong> nonMatching = new List<ulong>();
            foreach (ulong key in g1.Connections.Keys)
            {
                if (g2.Connections.ContainsKey(key))
                    matching.Add(new Tuple<ConnectionGene, ConnectionGene>(g1.Connections[key], g2.Connections[key]));
                else
                    nonMatching.Add(key); // Find all Disjoint & Excess from Parent 1
            }
            // W = avg weight diff of matching genes
            float W = 0;
            for (int i = 0; i < matching.Count; i++)
            {
                Tuple<ConnectionGene, ConnectionGene> t = matching[i];
                W += Math.Abs(t.Item1.Weight - t.Item2.Weight);
            }
            W /= matching.Count;
            // E = Number of Excess Genes
            // D = Number of Disjoint Genes
            uint E = 0;
            uint D = 0;
            Tuple<uint, uint> tup = GetExcessDisjoint(nonMatching, g2); // Check Excess/Disjoint Parent 1
            E += tup.Item1;
            D += tup.Item2;
            nonMatching.Clear();
            foreach (ulong key in g2.Connections.Keys)
                if (!g1.Connections.ContainsKey(key))
                    nonMatching.Add(key); // Get Excess/Disjoint Parent 2
            tup = GetExcessDisjoint(nonMatching, g1);
            E += tup.Item1;
            D += tup.Item2;
            return (c1 * E) / N + (c2 * D) / N + c3 * W;
        }
        /// <summary>
        /// Gets amount of Excess & Disjoint Genes in a Genome
        /// </summary>
        /// <param name="nonMatching">List of Genes to Check</param>
        /// <param name="genome">Genome to check list against</param>
        /// <returns>Tuple with amount of Excess (Item1) and Disjoint (Item2) Genes</returns>
        private static Tuple<uint, uint> GetExcessDisjoint(List<ulong> nonMatching, Genome genome)
        {
            uint disjoint = 0, excess = 0;
            List<ulong> innos = genome.Connections.Keys.ToList();
            innos.Sort();
            ulong finalInno = innos.Last();
            for (int i = 0; i < nonMatching.Count; i++)
            {
                if (nonMatching[i] < finalInno)
                    disjoint++;
                else
                    excess++;
            }
            return new Tuple<uint, uint>(excess, disjoint);
        }
        #endregion
        #endregion
    }
}