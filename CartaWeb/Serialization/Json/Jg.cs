using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;

namespace CartaWeb.Serialization.Json
{
    /// <summary>
    /// Represents a single or multiple graph in JSON Graph format.
    /// </summary>
    /// <remarks>
    /// This class allows for easy serialization and deserialization to and from JSON specified by the JSON Graph
    /// Format.
    /// </remarks>
    public class JgFormat
    {
        /// <summary>
        /// The single graph.
        /// </summary>
        [JsonPropertyName("graph")]
        public JgFormatGraph GraphSingle { get; set; }
        /// <summary>
        /// The multiple graphs.
        /// </summary>
        [JsonPropertyName("graphs")]
        public List<JgFormatGraph> GraphsMultiple { get; set; }

        /// <summary>
        /// Constructs the graph.
        /// </summary>
        [JsonIgnore]
        public MemoryGraph Graph
        {
            get
            {
                // Check that the appropriate property exists first.
                // Then, return the graph.
                if (GraphSingle is null) return null;
                return GraphSingle.Graph;
            }
        }
        /// <summary>
        /// Constructs the graphs.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<MemoryGraph> Graphs
        {
            get
            {
                // Check that the appropriate property exists first.
                // Then, return the list of graph.
                if (GraphsMultiple is null) return null;
                return GraphsMultiple.Select(graph => graph.Graph);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormat"/> class.
        /// </summary>
        public JgFormat() { }

        /// <summary>
        /// Constructs a <see cref="JgFormat"/> version of the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to JG format.</param>
        /// <returns>The JG formatted graph.</returns>
        public static async Task<JgFormat> CreateAsync(IGraph graph)
        {
            JgFormat jgFormat = new();
            jgFormat.GraphSingle = await JgFormatGraph.CreateAsync(graph);
            return jgFormat;
        }
        /// <summary>
        /// Constructs a <see cref="JgFormat"/> version of the specified graphs.
        /// </summary>
        /// <param name="graphs">The enumerable of graphs to convert to JG format.</param>
        /// <returns>The JG formatted graphs.</returns>
        public static async Task<JgFormat> CreateAsync(IEnumerable<IGraph> graphs)
        {
            JgFormat jgFormat = new();
            jgFormat.GraphsMultiple = new List<JgFormatGraph>
            (
                await Task.WhenAll(graphs.Select(async graph => await JgFormatGraph.CreateAsync(graph)))
            );
            return jgFormat;
        }
    }

    /// <summary>
    /// Represents a graph in JSON Graph format.
    /// </summary>
    public class JgFormatGraph
    {
        /// <summary>
        /// The graph vertices.
        /// </summary>
        [JsonPropertyName("nodes")]
        public Dictionary<string, JgFormatNode> Nodes { get; set; }
        /// <summary>
        /// The graph edges.
        /// </summary>
        [JsonPropertyName("edges")]
        public List<JgFormatEdge> Edges { get; set; }

        /// <summary>
        /// Constructs the graph.
        /// </summary>
        [JsonIgnore]
        public MemoryGraph Graph
        {
            get
            {
                // Create a graph and add the vertices and edges.
                MemoryGraph graph = new(null);
                graph.AddVertexRange(Nodes.Select(pair => new Vertex
                (
                    pair.Key,
                    pair.Value.Properties
                )
                {
                    Label = pair.Value.Label,
                    Description = pair.Value.Description
                }));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatGraph"/> class.
        /// </summary>
        public JgFormatGraph() { }

        /// <summary>
        /// Constructes a <see cref="JgFormatGraph"/> version of the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to JG format.</param>
        /// <returns>The JG formatted graph.</returns>
        public static async Task<JgFormatGraph> CreateAsync(IGraph graph)
        {
            JgFormatGraph jgFormatGraph = new();

            // Read the content of the graph.
            if (graph.Components.TryFind(out IEnumerableComponent<IVertex, IEdge> enumerableComponent))
            {
                // Initialize the nodes and edges.
                jgFormatGraph.Nodes = new Dictionary<string, JgFormatNode>();
                jgFormatGraph.Edges = new List<JgFormatEdge>();

                await foreach (IVertex vertex in enumerableComponent.GetVertices())
                {
                    // Add the vertex to the nodes.
                    jgFormatGraph.Nodes.Add(vertex.Id, new JgFormatNode(vertex));

                    // Add each of the edges to the graph.
                    foreach (IEdge edge in vertex.Edges)
                        jgFormatGraph.Edges.Add(new JgFormatEdge(edge));
                }
            }

            return jgFormatGraph;
        }
    }

    /// <summary>
    /// Represents a vertex in JSON Graph format.
    /// </summary>
    public class JgFormatNode
    {
        /// <summary>
        /// The vertex label which is visible on vertex visualization.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }
        /// <summary>
        /// The vertex description which pops up on vertex visualization.
        /// </summary>
        [JsonPropertyName("title")]
        public string Description { get; set; }

        /// <summary>
        /// The vertex metadata.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Constructs the properties.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, IProperty> Properties
        {
            get
            {
                // Check for no metadata before trying to read it.
                if (Metadata is null)
                    return new Dictionary<string, IProperty>();

                // Return the metadata parsed into a sorted list.
                return Metadata.ToDictionary(
                    pair => (string)pair.Key,
                    pair => (IProperty)new Property(pair.Value)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatNode"/> class with the specified node.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        public JgFormatNode(IVertex vertex)
        {
            if (vertex is IElement element)
            {
                Label = element.Label;
                Description = element.Description;
            }
            if (vertex is IProperty property)
            {
                Metadata = property.Properties
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value.Value
                    );
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatNode"/> class.
        /// </summary>
        public JgFormatNode() { }
    }

    /// <summary>
    /// Represents a edge in JSON Graph format.
    /// </summary>
    public class JgFormatEdge
    {
        /// <summary>
        /// The edge identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The source vertex identifier.
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }
        /// <summary>
        /// The target vertex identifier.
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; set; }
        /// <summary>
        /// Whether the edge is directed.
        /// </summary>
        [JsonPropertyName("directed")]
        public bool Directed { get; set; }

        /// <summary>
        /// Constructs the edge.
        /// </summary>
        [JsonIgnore]
        public Edge Edge => new(Source, Target);

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatEdge"/> class with the specified edge.
        /// </summary>
        /// <param name="edge">The edge to convert to a new format.</param>
        public JgFormatEdge(IEdge edge)
        {
            Id = edge.Id;
            Source = edge.Source;
            Target = edge.Target;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatEdge"/> class.
        /// </summary>
        public JgFormatEdge() { }
    }
}