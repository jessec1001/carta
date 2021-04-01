using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;

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
        /// Gets or sets the single graph.
        /// </summary>
        /// <value>
        /// The single graph.
        /// </value>
        [JsonPropertyName("graph")]
        public JgFormatGraph GraphSingle { get; set; }
        /// <summary>
        /// Gets or sets the multiple graphs.
        /// </summary>
        /// <value>
        /// The multiple graphs.
        /// </value>
        [JsonPropertyName("graphs")]
        public List<JgFormatGraph> GraphsMultiple { get; set; }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        /// <value>
        /// The graph.
        /// </value>
        [JsonIgnore]
        public FiniteGraph Graph
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
        /// Gets the graphs.
        /// </summary>
        /// <value>
        /// The graphs.
        /// </value>
        [JsonIgnore]
        public IEnumerable<FiniteGraph> Graphs
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
        public static async Task<JgFormat> CreateAsync(IEntireGraph graph)
        {
            JgFormat jgFormat = new JgFormat();
            jgFormat.GraphSingle = await JgFormatGraph.CreateAsync(graph);
            return jgFormat;
        }
        /// <summary>
        /// Constructs a <see cref="JgFormat"/> version of the specified graphs.
        /// </summary>
        /// <param name="graphs">The enumerable of graphs to convert to JG format.</param>
        /// <returns>The JG formatted graphs.</returns>
        public static async Task<JgFormat> CreateAsync(IEnumerable<IEntireGraph> graphs)
        {
            JgFormat jgFormat = new JgFormat();
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
        /// Gets or sets the whether the graph is directed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the graph is directed; otherwise <c>false</c>.
        /// </value>
        [JsonPropertyName("directed")]
        public bool Directed { get; set; }

        /// <summary>
        /// Gets or sets the graph nodes.
        /// </summary>
        /// <value>
        /// The map of identifier-node pairs of graph nodes.
        /// </value>
        [JsonPropertyName("nodes")]
        public Dictionary<string, JgFormatNode> Nodes { get; set; }
        /// <summary>
        /// Gets or sets the graph edges.
        /// </summary>
        /// <value>
        /// The graph edges.
        /// </value>
        [JsonPropertyName("edges")]
        public List<JgFormatEdge> Edges { get; set; }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        /// <value>
        /// The graph.
        /// </value>
        [JsonIgnore]
        public FiniteGraph Graph
        {
            get
            {
                // Create a graph and add the vertices and edges.
                FiniteGraph graph = new FiniteGraph(null, Directed);
                graph.AddVertexRange(Nodes.Select(pair => new Vertex
                (
                    Identity.Create(pair.Key),
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
        public static async Task<JgFormatGraph> CreateAsync(IEntireGraph graph)
        {
            JgFormatGraph jgFormatGraph = new JgFormatGraph();

            jgFormatGraph.Directed = graph.IsDirected;
            jgFormatGraph.Nodes = await graph.Vertices.ToDictionaryAwaitAsync
            (
                node => new ValueTask<string>(Task.FromResult(node.Identifier.ToString())),
                node => new ValueTask<JgFormatNode>(Task.FromResult(new JgFormatNode(node)))
            );
            jgFormatGraph.Edges = await graph.Edges.SelectAwait
            (
                edge => new ValueTask<JgFormatEdge>(Task.FromResult(new JgFormatEdge(edge)))
            ).ToListAsync();

            return jgFormatGraph;
        }
    }

    /// <summary>
    /// Represents a vertex in JSON Graph format.
    /// </summary>
    public class JgFormatNode
    {
        /// <summary>
        /// Gets or sets the vertex label.
        /// </summary>
        /// <value>
        /// The vertex label which is visible on vertex visualization.
        /// </value>
        [JsonPropertyName("label")]
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the vertex description.
        /// </summary>
        /// <value>
        /// The vertex description which pops up on vertex visualization.
        /// </value>
        [JsonPropertyName("title")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the vertex metadata.
        /// </summary>
        /// <value>
        /// The vertex metadata.
        /// </value>
        [JsonPropertyName("metadata")]
        public Dictionary<string, List<object>> Metadata { get; set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        [JsonIgnore]
        public List<Property> Properties
        {
            get
            {
                // Check for no metadata before trying to read it.
                if (Metadata is null)
                    return new List<Property>();

                // Return the metadata parsed into a sorted list.
                return Metadata
                    .Select(pair => new Property
                    (
                        Identity.Create(pair.Key),
                        pair.Value.ToList()
                    )).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatNode"/> class with the specified node.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        public JgFormatNode(IVertex vertex)
        {
            Label = vertex.Label;
            Description = vertex.Description;

            if (vertex.Properties is not null)
            {
                Metadata = vertex.Properties
                    .ToDictionary(
                        property => property.Identifier.ToString(),
                        property => property.Values.ToList()
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
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The edge ID.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the source vertex ID.
        /// </summary>
        /// <value>
        /// The source vertex ID.
        /// </value>
        [JsonPropertyName("source")]
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the target vertex ID.
        /// </summary>
        /// <value>
        /// The target vertex ID.
        /// </value>
        [JsonPropertyName("target")]
        public string Target { get; set; }

        /// <summary>
        /// Gets the edge.
        /// </summary>
        /// <value>
        /// The edge.
        /// </value>
        [JsonIgnore]
        public Edge Edge
        {
            get
            {
                return new Edge
                (
                    Identity.Create(Source),
                    Identity.Create(Target)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatEdge"/> class with the specified edge.
        /// </summary>
        /// <param name="edge">The edge to convert to a new format.</param>
        public JgFormatEdge(Edge edge)
        {
            Id = edge.Identifier.ToString();
            Source = edge.Source.ToString();
            Target = edge.Target.ToString();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatEdge"/> class.
        /// </summary>
        public JgFormatEdge() { }
    }
}