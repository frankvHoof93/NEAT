using nl.FvH.NEAT.Brain.Genes;
using nl.FvH.NEAT.Enums;
using nl.FvH.NEAT.Factories;
using nl.FvH.NEAT.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nl.FvH.NEAT.Brain
{
    public class Genome
    {
        #region Variables
        #region Constants
        /// <summary>
        /// Max amount of attempts at finding nodes/connections during Mutation
        /// </summary>
        private const uint MaxFindAttempts = 20;
        #endregion

        #region Public
        /// <summary>
        /// Number of ConnectionGenes in Genome
        /// </summary>
        public uint NumGenes => (uint)Connections.Count;
        /// <summary>
        /// Fitness for Genome
        /// </summary>
        public float Fitness => fitness ?? 0;
        #endregion

        #region Private
        /// <summary>
        /// ConnectionGenes in Genome (by Innovation-Number)
        /// </summary>
        internal readonly Dictionary<ulong, ConnectionGene> Connections = new Dictionary<ulong, ConnectionGene>();
        /// <summary>
        /// NodeGenes in Genome (by Innovation-Number)
        /// </summary>
        internal readonly Dictionary<ulong, NodeGene> Nodes = new Dictionary<ulong, NodeGene>();
        /// <summary>
        /// Fitness for Genome
        /// </summary>
        private float? fitness;
        /// <summary>
        /// Ordered List of NodeGene-Innovation-Numbers
        /// </summary>
        private List<ulong> nodesInOrder = new List<ulong>();
        /// <summary>
        /// Connections for each Node (Connections where Node is Output)
        /// </summary>
        private readonly Dictionary<NodeGene, List<ConnectionGene>> inputConnectionsPerNode = new Dictionary<NodeGene, List<ConnectionGene>>();
        /// <summary>
        /// Cached Array for Outputs (to re-use memory)
        /// </summary>
        private double[] outputCache;
        #endregion
        #endregion

        #region Methods
        #region Constructors
        /// <summary>
        /// Constructor for a Genome with Nodes & Connections
        /// </summary>
        /// <param name="genomeNodes">Nodes for Genome</param>
        /// <param name="genomeConnections">Connections for Genome</param>
        internal Genome(List<NodeGene> genomeNodes, List<ConnectionGene> genomeConnections)
        {
            for (int i = 0; i < genomeNodes.Count; i++)
            {
                NodeGene g = genomeNodes[i];
                Nodes.Add(g.Innovation, g);
            }
            for (int i = 0; i < genomeConnections.Count; i++)
            {
                ConnectionGene g = genomeConnections[i];
                Connections.Add(g.Innovation, g);
            }
        }
        /// <summary>
        /// Constructor for an Empty Genome (used to create Child-Genome during Breeding)
        /// </summary>
        private Genome() { }
        #endregion

        #region Calculation
        /// <summary>
        /// Calculates Output for Genome
        /// </summary>
        /// <param name="input">Input for Genome</param>
        /// <returns>Output-Values for Genome</returns>
        public double[] Calculate(double[] input)
        {
            int output = 0;
            if (inputConnectionsPerNode.Count == 0)
                SetUpNetwork();
            for (int i = 0; i < nodesInOrder.Count; i++)
            {
                NodeGene gene = Nodes[nodesInOrder[i]];
                if (gene.Type == NodeType.INPUT) // Input-Nodes are always at the start, as they have the lowest Innovation-Number
                    gene.SetState(input[i]);
                else
                {
                    List<ConnectionGene> inputs = inputConnectionsPerNode[gene];
                    double N = 0;
                    for (int j = 0; j < inputs.Count; j++)
                    {
                        ConnectionGene conn = inputs[j];
                        NodeGene inNode = Nodes[conn.In.Innovation]; // Grab Node from Nodes to get proper State (We're using Structs)
                        N += conn.Weight * inNode.State;
                    }
                    gene.SetState(Activations.Sigmoid(N));
                    if (gene.Type == NodeType.OUTPUT)
                        outputCache[output] = gene.State;
                }
            }
            return outputCache;
        }
        /// <summary>
        /// Sets up variables used during Calculation of Output (to speed up calculation)
        /// </summary>
        private void SetUpNetwork()
        {
            int numOutputs = 0;
            nodesInOrder = Nodes.Keys.ToList();
            nodesInOrder.Sort();
            for (int i = 0; i < nodesInOrder.Count; i++)
                if (Nodes[nodesInOrder[i]].Type == NodeType.OUTPUT)
                    numOutputs++;
            outputCache = new double[numOutputs];
            foreach (ConnectionGene connection in Connections.Values)
            {
                NodeGene node = connection.Out;
                if (!connection.Expressed)
                    continue;
                if (inputConnectionsPerNode.ContainsKey(node))
                    inputConnectionsPerNode[node].Add(connection);
                else
                    inputConnectionsPerNode.Add(node, new List<ConnectionGene> { connection });
            }
        }
        #endregion

        #region Fitness
        /// <summary>
        /// Sets Fitness for Genome
        /// </summary>
        /// <param name="fit">Fitness to set</param>
        public void SetFitness(float fit)
        {
            fitness = fit;
        }
        #endregion

        #region Mutation
        /// <summary>
        /// Mutates this Genome
        /// </summary>
        internal void Mutate()
        {
            float m = Functions.GetRandomNumber(0f, 1f);
            int mutations = 1;
            if (m > 0.9f)
                mutations = 3;
            else if (m > 0.66f)
                mutations = 2;
            for (int i = 0; i < mutations; i++)
            {
                int choice = Functions.GetRandomNumber(0, 3);
                switch (choice)
                {
                    case 0:
                        MutateAddRandomConnection();
                        break;
                    case 1:
                        MutateAddRandomNode();
                        break;
                    case 2:
                        PerturbRandomConnections();
                        break;
                    default:
                        break;
                }
            }
        }

        #region Structure
        #region Connections
        /// <summary>
        /// Adds random connection (Structural Mutation) to Genome
        /// </summary>
        internal void MutateAddRandomConnection()
        {
            if (Nodes.Count < 2)
                throw new InvalidOperationException("Not enough Existing Nodes");
            // Separate out the Nodes by Type
            List<NodeGene> inputNodes = new List<NodeGene>();
            List<NodeGene> hiddenNodes = new List<NodeGene>();
            List<NodeGene> outputNodes = new List<NodeGene>();
            foreach (NodeGene node in Nodes.Values)
                switch (node.Type)
                {
                    case NodeType.INPUT:
                        inputNodes.Add(node);
                        break;
                    case NodeType.HIDDEN:
                        hiddenNodes.Add(node);
                        break;
                    case NodeType.OUTPUT:
                        outputNodes.Add(node);
                        break;
                    default:
                        break;
                }
            // Add hiddenNodes to both lists to create the choice-lists
            inputNodes.AddRange(hiddenNodes);
            outputNodes.AddRange(hiddenNodes);




            // Find 2 nodes in these 2 lists which are NOT connected
            NodeGene inputNode = inputNodes[Functions.GetRandomNumber(0, inputNodes.Count)]; // Pick random inputNode
            int attempts = 0;
            while (attempts < MaxFindAttempts) // TODO: Improve this
            {
                attempts++;
                NodeGene outputNode = outputNodes[Functions.GetRandomNumber(0, inputNodes.Count)];
                if (inputNode.Equals(outputNode)) // Same (hidden) node, can't connect to self
                    continue;
                bool foundConnection = false;
                foreach (ConnectionGene conn in Connections.Values)
                    if (conn.HasNode(inputNode) && conn.HasNode(outputNode))
                    {
                        foundConnection = true;
                        break; // Connection Exists
                    }
                if (foundConnection)
                    continue; // failed to find valid pair
                // InputNode != OutputNode and !(I->O) && !(O->I)
                AddConnection(MutationFactory.GetConnection(inputNode, outputNode));
                return;
            }
            Console.WriteLine("Failed to add Connection");
        }
        /// <summary>
        /// Adds Connection (Structural Mutation) to Genome from Nodes
        /// </summary>
        /// <param name="nodeFrom">From-Node (Innovation-Number) for Connection</param>
        /// <param name="nodeTo">To-Node (Innovation-Number) for Connection</param>
        internal void MutateAddConnection(ulong nodeFrom, ulong nodeTo)
        {
            if (nodeFrom == 0)
                throw new ArgumentException("NodeInnovation cannot be 0", "nodeFrom");
            if (nodeTo == 0)
                throw new ArgumentException("NodeInnovation cannot be 0", "nodeTo");
            if (nodeFrom == nodeTo)
                throw new ArgumentException("Cannot create Connection with a single Node");
            if (!Nodes.ContainsKey(nodeFrom))
                throw new ArgumentException("Node does not exist in Genome", "nodeFrom");
            if (!Nodes.ContainsKey(nodeTo))
                throw new ArgumentException("Node does not exist in Genome", "nodeTo");
            NodeGene from = Nodes[nodeFrom];
            NodeGene to = Nodes[nodeTo];
            if (from.Type == NodeType.OUTPUT || to.Type == NodeType.INPUT)
                throw new InvalidOperationException($"Invalid NodeTypes for Connection. Type NodeFrom: {from.Type} Type NodeTo: {to.Type}");
            // TODO: Check if Connection does not exist already in Genome
            AddConnection(MutationFactory.GetConnection(from, to));
        }
        /// <summary>
        /// Adds Connection (Structural Mutation) to Genome from Nodes
        /// </summary>
        /// <param name="nodeFrom">From-Node for Connection</param>
        /// <param name="nodeTo">To-Node for Connection</param>
        internal void MutateAddConnection(NodeGene nodeFrom, NodeGene nodeTo)
        {
            if (!nodeFrom.IsValid)
                throw new ArgumentException("NodeFrom is Invalid", "nodeFrom");
            if (!nodeTo.IsValid)
                throw new ArgumentException("NodeTo is Invalid", "nodeTo");
            if (nodeFrom.Equals(nodeTo))
                throw new ArgumentException("Cannot create Connection with a single Node");
            if (!Nodes.ContainsKey(nodeFrom.Innovation))
                throw new ArgumentException("Node does not exist in Genome", "nodeFrom");
            if (!Nodes.ContainsKey(nodeTo.Innovation))
                throw new ArgumentException("Node does not exist in Genome", "nodeTo");
            if (nodeFrom.Type == NodeType.OUTPUT || nodeTo.Type == NodeType.INPUT)
                throw new InvalidOperationException($"Invalid NodeTypes for Connection. Type NodeFrom: {nodeFrom.Type} Type NodeTo: {nodeTo.Type}");
            // TODO: Check if Connection does not exist already in Genome
            AddConnection(MutationFactory.GetConnection(nodeFrom, nodeTo));
        }
        /// <summary>
        /// Adds a ConnectionGene to this Genome. Overwrites if ConnectionGene with Innovation-Number exists
        /// </summary>
        /// <param name="gene">ConnectionGene to add</param>
        private void AddConnection(ConnectionGene gene)
        {
            if (!gene.IsValid)
                throw new ArgumentException("Connection is not Valid", "gene");
            if (Connections.ContainsKey(gene.Innovation))
                Connections[gene.Innovation] = gene;
            else
                Connections.Add(gene.Innovation, gene);
            AddNode(gene.In);
            AddNode(gene.Out);
        }
        #endregion

        #region Nodes
        /// <summary>
        /// Adds Node for Random Connection in Genome (Structural Mutation)
        /// </summary>
        internal void MutateAddRandomNode()
        {
            if (Connections.Count == 0)
                throw new InvalidOperationException("No existing connections");
            // Find a random (expressed) connection
            List<ConnectionGene> conns = Connections.Values.ToList();
            for (int i = 0; i < MaxFindAttempts; i++) // Try x times max
            {
                int randomIndex = Functions.GetRandomNumber(0, Connections.Count);
                ConnectionGene conn = conns[randomIndex];
                if (!conn.Expressed)
                    continue; // Connection is not active
                MutateAddNode(conn); // Add Node for Connection
                return; // Break out (solution found)
            }
            Console.WriteLine("Failed to add Node");
        }
        /// <summary>
        /// Adds Node for Connection (Structural Mutation)
        /// </summary>
        /// <param name="connectionInnovation">Connection to add Node for</param>
        internal void MutateAddNode(ConnectionGene conn)
        {
            if (!conn.IsValid)
                throw new ArgumentException("Connection is not Valid", "conn");
            if (!conn.Expressed)
                return; // Connection is not active
            ConnectionGene conn1, conn2;
            NodeGene newNode = MutationFactory.GetNode(ref conn, out conn1, out conn2);
            // Overwrite old Connection (now disabled)
            Connections[conn.Innovation] = conn;
            // Add new Objects
            AddNode(newNode);
            AddConnection(conn1);
            AddConnection(conn2);
        }
        /// <summary>
        /// Adds Node for Connection (Structural Mutation) by Connection-InnovationNumber
        /// </summary>
        /// <param name="connectionInnovation">Innovation-Number for Connection</param>
        internal void MutateAddNode(ulong connectionInnovation)
        {
            if (Connections.ContainsKey(connectionInnovation))
                MutateAddNode(Connections[connectionInnovation]);
            else
                throw new ArgumentException("Connection for Innovation-Number does not exist in Genome", "connectionInnovation");
        }
        /// <summary>
        /// Adds a NodeGene to this Genome. Overwrites if NodeGene with Innovation-Number exists
        /// </summary>
        /// <param name="gene">NodeGene to add</param>
        private void AddNode(NodeGene gene)
        {
            if (!gene.IsValid)
                throw new ArgumentException("Node is not Valid", "gene");
            if (Nodes.ContainsKey(gene.Innovation))
                Nodes[gene.Innovation] = gene;
            else
                Nodes.Add(gene.Innovation, gene);
        }
        #endregion
        #endregion

        #region Weights
        /// <summary>
        /// Randomly perturbs 50% of the Connection-Weights in this Genome
        /// </summary>
        internal void PerturbRandomConnections()
        {
            List<ulong> keys = Connections.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                if (Functions.GetRandomNumber(0f, 1f + float.Epsilon) < 0.5f)
                    continue; // 50/50 on whether to perturb
                float perturb = Functions.GetRandomNumber(0f, 1f + float.Epsilon) > 0.8 ? -1 : 1; // Randomize whether to 'flip' the weight (Bias for No)
                perturb *= Functions.GetRandomNumber(0.75f, 1.25f + float.Epsilon); // Perturb between (+ or -) .75 and 1.25 * original value
                Connections[keys[i]].PerturbWeight(perturb);
            }
        }
        #endregion
        #endregion

        #region Breeding
        /// <summary>
        /// Breeds 2 Genomes to create a Child
        /// </summary>
        /// <param name="parent1">Parent 1 for Breeding</param>
        /// <param name="parent2">Parent 2 for Breeding</param>
        /// <returns>Child Genome</returns>
        internal static Genome Breed(Genome parent1, Genome parent2)
        {
            bool equalFitness = false;
            if (parent1.Fitness > parent2.Fitness) // Make sure Parent1 is the more fit parent
            {
                Genome temp = parent1;
                parent1 = parent2;
                parent2 = temp;
            }
            else if (parent1.Fitness == parent1.Fitness)
                equalFitness = true;
            // Find all matching & non-matching Connections via Innovation-Numbers
            List<ulong> matching = new List<ulong>();
            List<ulong> nonMatching = new List<ulong>();
            foreach (ulong key in parent1.Connections.Keys)
            {
                if (parent2.Connections.ContainsKey(key))
                    matching.Add(key);
                else
                    nonMatching.Add(key); // Find all Disjoint & Excess from Parent 1
            }
            if (equalFitness)
                foreach (ulong key in parent2.Connections.Keys)
                    if (!parent1.Connections.ContainsKey(key))
                        nonMatching.Add(key); // Find all Disjoint & Excess from Parent 2 (Matching fitness)
            // Create child
            Genome child = new Genome();
            // Matching genes are inherited randomly
            for (int i = 0; i < matching.Count; i++)
            {
                ulong inno = matching[i];
                ConnectionGene gene = Functions.GetRandomNumber(0f, 1f + float.Epsilon) < 0.5 ? parent1.Connections[inno] : parent2.Connections[inno]; // Pick random
                child.AddConnection(gene);
            }
            // Disjoint & Excess Genes:
            for (int i = 0; i < nonMatching.Count; i++)
            {
                ulong inno = nonMatching[i];
                if (parent1.Connections.ContainsKey(inno)) // Disjoint and Excess are inherited from the most fit parent
                    child.AddConnection(parent1.Connections[inno]);
                else if (equalFitness) // If fitness is equal, disjoint and excess are inherited from both parents
                    child.AddConnection(parent2.Connections[inno]);
            }
            return child;
        }
        #endregion
        #endregion
    }
}
