using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="HubGraphOperation" /> operation.
    /// </summary>
    public struct HubGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the hub graph.
        /// </summary>
        [FieldRange(Minimum = 1)]
        [FieldName("Vertex Count")]
        public int VertexCount { get; set; }
        /// <summary>
        /// Whether the resulting graph should be directed or undirected.
        /// </summary>
        [FieldName("Directed")]
        public bool Directed { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="HubGraphOperation" /> operation.
    /// </summary>
    public struct HubGraphOperationOut
    {
        /// <summary>
        /// The generated hub graph.
        /// </summary>
        [FieldName("Graph")]
        public MemoryGraph Graph { get; set; }
    }
    /// <summary>
    /// Generates a directed or undirected hub graph with the specified number of vertices. The hub graph contains one
    /// central vertex with all remaining vertices with edges connected to it. If the edges are directed, they will
    /// point from the central vertex.
    /// </summary>
    [OperationName(Display = "Generate Hub Graph", Type = "generateHubGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
    public class HubGraphOperation : TypedOperation
    <
        HubGraphOperationIn,
        HubGraphOperationOut
    >
    {
        /// >inheritdoc />
        public override Task<HubGraphOperationOut> Perform(HubGraphOperationIn input)
        {
            // Create the graph structure.
            MemoryGraph graph = new($"H_{input.VertexCount}");

            // Generate the vertices of the graph.
            List<Edge> centralEdges = new();
            for (int k = 1; k < input.VertexCount; k++)
            {
                Edge edge = new("0", k.ToString()) { Directed = input.Directed };
                Vertex vertex = new(k.ToString(), new Edge[] { edge });

                centralEdges.Add(edge);
                graph.AddVertex(vertex);
            }
            Vertex centralVertex = new("0", centralEdges);
            graph.AddVertex(centralVertex);

            return Task.FromResult(new HubGraphOperationOut { Graph = graph });
        }
    }
}