using nl.FvH.NEAT.Factories;
using System;

namespace nl.FvH.NEAT.Brain.Genes
{
    internal struct ConnectionGene : IEquatable<ConnectionGene>, IComparable<ConnectionGene>
    {
        #region Variables
        /// <summary>
        /// Innovation-Number for Gene
        /// </summary>
        public readonly ulong Innovation;
        /// <summary>
        /// Starting (Input-)Gene for Connection
        /// </summary>
        public readonly NodeGene In;
        /// <summary>
        /// Ending (Output-)Gene for Connection
        /// </summary>
        public readonly NodeGene Out;
        /// <summary>
        /// Checks whether this ConnectionGene is Valid (Innovation != 0)
        /// </summary>
        public bool IsValid => Innovation != 0;
        /// <summary>
        /// Connection-Weight
        /// </summary>
        public float Weight { get; private set; }
        /// <summary>
        /// Is the Connection Expressed (used)?
        /// </summary>
        public bool Expressed { get; private set; }
        #endregion

        #region Methods
        #region Constructor
        /// <summary>
        /// Constructor for a ConnectionGene
        /// </summary>
        /// <param name="inNode"></param>
        /// <param name="outNode"></param>
        /// <param name="weight"></param>
        /// <param name="expressed"></param>
        /// <param name="innovation"></param>
        internal ConnectionGene(NodeGene inNode, NodeGene outNode, float weight, bool expressed = true, ulong innovation = 0)
        {
            Innovation = innovation == 0 ? InnovationFactory.GetConnectionInnovation() : innovation;
            In = inNode;
            Out = outNode;
            Weight = weight;
            Expressed = expressed;
        }
        #endregion

        #region Public
        /// <summary>
        /// Disables the Connection, meaning it is no longer Expressed
        /// </summary>
        public void Disable()
        {
            Expressed = false;
        }
        /// <summary>
        /// Perturbs Weight for Connection
        /// </summary>
        /// <param name="factor">Factor to perturb by</param>
        public void PerturbWeight(float factor)
        {
            Weight *= factor;
        }
        /// <summary>
        /// Checks whether a Node is part of this Connection (as either Input or Output) by Innovation-Number
        /// </summary>
        /// <param name="node">Node to Check for</param>
        /// <returns>True if <c ref="In">In</c> or <c ref="Out">Out</c> have an Equal Innovation-Number to <paramref name="node"/></returns>
        public bool HasNode(NodeGene node)
        {
            return In.Equals(node) || Out.Equals(node);
        }
        #endregion

        #region Interface
        /// <summary>
        /// Compares ConnectionGenes by Innovation-Number
        /// </summary>
        /// <param name="other">ConnectionGene to Compare against</param>
        /// <returns>CompareTo of Innovation-Numbers</returns>
        public int CompareTo(ConnectionGene other)
        {
            return Innovation.CompareTo(other.Innovation);
        }

        /// <summary>
        /// Compares ConnectionGenes by Innovation-Number
        /// </summary>
        /// <param name="other">ConnectionGene to Compare against</param>
        /// <returns>True if Innovation-Numbers are equal</returns>
        public bool Equals(ConnectionGene other)
        {
            return Innovation == other.Innovation;
        }
        #endregion
        #endregion
    }
}