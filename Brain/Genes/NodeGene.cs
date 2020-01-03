using nl.FvH.NEAT.Enums;
using nl.FvH.NEAT.Factories;
using System;

namespace nl.FvH.NEAT.Brain.Genes
{
    internal struct NodeGene : IEquatable<NodeGene>, IComparable<NodeGene>
    {
        #region Variables
        /// <summary>
        /// Innovation-Number for Gene
        /// </summary>
        public readonly ulong Innovation;
        /// <summary>
        /// Type for NodeGene (Layer-Type)
        /// </summary>
        public readonly NodeType Type;
        /// <summary>
        /// Checks whether this NodeGene is Valid (Innovation != 0)
        /// </summary>
        public bool IsValid => Innovation != 0;
        #endregion

        #region Methods
        /// <summary>
        /// Constructor for a NodeGene
        /// </summary>
        /// <param name="type">NodeType for Gene</param>
        /// <param name="innovation">Innovation-Number for Gene. Leave 0 to Auto-Generate</param>
        internal NodeGene(NodeType type, ulong innovation = 0)
        {
            Type = type;
            Innovation = innovation == 0 ? InnovationFactory.GetNodeInnovation() : innovation;
        }

        /// <summary>
        /// Compares NodeGenes by Innovation-Number
        /// </summary>
        /// <param name="other">NodeGene to Compare against</param>
        /// <returns>CompareTo of Innovation-Numbers</returns>
        public int CompareTo(NodeGene other)
        {
            return Innovation.CompareTo(other.Innovation);
        }

        /// <summary>
        /// Compares NodeGenes by Innovation-Number
        /// </summary>
        /// <param name="other">NodeGene to Compare against</param>
        /// <returns>True if Innovation-Numbers are equal</returns>
        public bool Equals(NodeGene other)
        {
            return Innovation == other.Innovation;
        }
        #endregion
    }
}