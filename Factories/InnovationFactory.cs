namespace nl.FvH.NEAT.Factories
{
    internal static class InnovationFactory
    {
        #region Variables
        /// <summary>
        /// (Current) Highest Innovation-Number for ConnectionGenes
        /// </summary>
        internal static ulong MaxConnection { get; private set; } = 0;
        /// <summary>
        /// (Current) Highest Innovation-Number for NodeGenes
        /// </summary>
        internal static ulong MaxNode { get; private set; } = 0;
        #endregion

        #region Methods
        /// <summary>
        /// Gets an Innovation-Number for a new ConnectionGene
        /// </summary>
        /// <returns>New Highest Innovation-Number for ConnectionGenes</returns>
        internal static ulong GetConnectionInnovation()
        {
            return ++MaxConnection;
        }
        /// <summary>
        /// Gets an Innovation-Number for a new NodeGene
        /// </summary>
        /// <returns>New Highest Innovation-Number for NodeGenes</returns>
        internal static ulong GetNodeInnovation()
        {
            return ++MaxNode;
        }
        #endregion
    }
}
