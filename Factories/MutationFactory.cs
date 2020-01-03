using nl.FvH.NEAT.Enums;
using nl.FvH.NEAT.Brain.Genes;
using System;
using System.Collections.Generic;
using nl.FvH.NEAT.Util;

namespace nl.FvH.NEAT.Factories
{
    /// <summary>
    /// A MutationFactory is used to ensure that when the same Mutation is created twice during a single Generation, these Mutations have the same Innovation-Number
    /// </summary>
    internal static class MutationFactory
    {
        #region Variables
        /// <summary>
        /// ConnectionGenes created this Generation
        /// </summary>
        private static readonly Dictionary<Tuple<ulong, ulong>, ConnectionGene> generationConnections = new Dictionary<Tuple<ulong, ulong>, ConnectionGene>();
        /// <summary>
        /// NodeGenes created this Generation
        /// </summary>
        private static readonly Dictionary<ulong, NodeGene> generationNodes = new Dictionary<ulong, NodeGene>();
        #endregion

        #region Methods
        /// <summary>
        /// Gets a (new) ConnectionGene for this Generation from In- & Out-Node
        /// </summary>
        /// <param name="inNode">In-Node for Connection</param>
        /// <param name="outNode">Out-Node for Connection</param>
        /// <returns>(New) ConnectionGene for Nodes</returns>
        internal static ConnectionGene GetConnection(NodeGene inNode, NodeGene outNode)
        {
            Tuple<ulong, ulong> hash = new Tuple<ulong, ulong>(inNode.Innovation, outNode.Innovation);
            if (generationConnections.ContainsKey(hash))
                return generationConnections[hash];
            ConnectionGene gene = new ConnectionGene(inNode, outNode, Functions.GetRandomNumber(-1f, 1f + float.Epsilon));
            generationConnections.Add(hash, gene);
            return gene;
        }
        /// <summary>
        /// Gets a (new) NodeGene for this Generation from existing ConnectionGene
        /// </summary>
        /// <param name="connectionOld">Connection to create Node for. Passed by reference in order to disable it</param>
        /// <param name="connectionNew1">OUT: New Connection from old In-Node to new Node</param>
        /// <param name="connectionNew2">OUT: New Connection from new Node to old Out-Node</param>
        /// <returns>Created NodeGene</returns>
        internal static NodeGene GetNode(ref ConnectionGene connectionOld, out ConnectionGene connectionNew1, out ConnectionGene connectionNew2)
        {
            connectionOld.Disable();
            NodeGene newNode;
            if (generationNodes.ContainsKey(connectionOld.Innovation)) // Exists
                newNode = generationNodes[connectionOld.Innovation];
            else
                newNode = new NodeGene(NodeType.HIDDEN); // New Node (Can only be a hidden node, as it is between other nodes)
            connectionNew1 = GetConnection(connectionOld.In, newNode); // Get/Create Connection from Old In to Gene
            connectionNew2 = GetConnection(newNode, connectionOld.Out); // Get/Create Connection from Gene to Old Out
            return newNode;
        }
        /// <summary>
        /// Ends Generation, clearing created Nodes & Connections for this Generation Factory
        /// </summary>
        internal static void EndGeneration()
        {
            generationConnections.Clear();
            generationNodes.Clear();
        }
        #endregion
    }
}
