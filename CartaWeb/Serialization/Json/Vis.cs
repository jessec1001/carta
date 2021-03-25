using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;

namespace CartaWeb.Serialization.Json
{
    /// <summary>
    /// Represents a graph in Vis format.
    /// </summary>
    /// <remarks>
    /// This class allows for easy serialization and deserialization to and from JSON. This format allows the
    /// serialized data to immediately be used by Vis.js.
    /// </remarks>
    public class VisFormat
    {
        /// <summary>
        /// Gets or sets the graph identifier.
        /// </summary>
        /// <value>The graph identifier.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets whether the graph is directed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the graph is directed; otherwise <c>false</c>.
        /// </value>
        [JsonPropertyName("directed")]
        public bool Directed { get; set; }
        /// <summary>
        /// Gets or sets whether the graph is dynamic.
        /// </summary>
        /// <value><c>true</c> if the graph is dynamic; otherwise <c>false</c>.</value>
        [JsonPropertyName("dynamic")]
        public bool Dynamic { get; set; }

        /// <summary>
        /// Gets or sets the graph nodes.
        /// </summary>
        /// <value>
        /// The graph nodes.
        /// </value>
        [JsonPropertyName("nodes")]
        public List<VisFormatNode> Nodes { get; set; }
        /// <summary>
        /// Gets or sets the graph edges.
        /// </summary>
        /// <value>
        /// The graph edges.
        /// </value>
        [JsonPropertyName("edges")]
        public List<VisFormatEdge> Edges { get; set; }

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
                FiniteGraph graph = new FiniteGraph(Identity.Create(Id), Directed);
                graph.AddVertexRange(Nodes.Select(node => node.Vertex));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormat"/> class.
        /// </summary>
        public VisFormat() { }

        /// <summary>
        /// Constructs a <see cref="VisFormat"/> version of the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to Vis format.</param>
        /// <returns>The Vis formatted graph.</returns>
        public static async Task<VisFormat> CreateAsync(IEntireGraph graph)
        {
            VisFormat visFormat = new VisFormat();

            if (graph is Graph baseGraph) visFormat.Id = baseGraph.Identifier.ToString();

            visFormat.Directed = graph.IsDirected;
            visFormat.Dynamic = graph.IsDynamic;
            visFormat.Nodes = await graph.Vertices.SelectAwait
            (
                node => new ValueTask<VisFormatNode>(Task.FromResult(new VisFormatNode(node)))
            ).ToListAsync();
            visFormat.Edges = await graph.Edges.SelectAwait
            (
                edge => new ValueTask<VisFormatEdge>(Task.FromResult(new VisFormatEdge(edge)))
            ).ToListAsync();

            return visFormat;
        }
    }

    /// <summary>
    /// Represents a node in Vis format.
    /// </summary>
    public class VisFormatNode
    {
        /// <summary>
        /// Gets or sets the node ID.
        /// </summary>
        /// <value>
        /// The node ID.
        /// </value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

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
        /// Gets or sets the vertex properties.
        /// </summary>
        /// <value>
        /// The vertex properties.
        /// </value>
        [JsonPropertyName("properties")]
        public List<VisFormatProperty> Properties { get; set; }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        /// <value>
        /// The vertex.
        /// </value>
        [JsonIgnore]
        public IVertex Vertex
        {
            get
            {
                return new Vertex
                (
                    Identity.Create(Id),
                    Properties?.Select(property => property.Property) ?? Enumerable.Empty<Property>()
                )
                {
                    Label = Label,
                    Description = Description
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatNode"/> class with the specified node.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        public VisFormatNode(IVertex vertex)
        {
            Id = vertex.Identifier.ToString();
            Label = vertex.Label;
            Description = string.IsNullOrEmpty(vertex.Description) ? null : vertex.Description;
            Properties = vertex.Properties?.Select(property => new VisFormatProperty(property)).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatNode"/> class.
        /// </summary>
        public VisFormatNode() { }
    }

    /// <summary>
    /// Represents an edge in Vis format.
    /// </summary>
    public class VisFormatEdge
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The edge ID.
        /// </value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the source vertex ID.
        /// </summary>
        /// <value>
        /// The source vertex ID.
        /// </value>
        [JsonPropertyName("from")]
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the target vertex ID.
        /// </summary>
        /// <value>
        /// The target vertex ID.
        /// </value>
        [JsonPropertyName("to")]
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
        /// Initializes a new instance of the <see cref="VisFormatEdge"/> class with the specified ID and endpoints.
        /// </summary>
        /// <param name="edge">The edge source and target to convert to a new format.</param>
        public VisFormatEdge(Edge edge)
        {
            Id = edge.Identifier.ToString();
            Source = edge.Source.ToString();
            Target = edge.Target.ToString();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatEdge"/> class.
        /// </summary>
        public VisFormatEdge() { }
    }

    /// <summary>
    /// Represents a vertex property in Vis format.
    /// </summary>
    public class VisFormatProperty
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the observations of the property.
        /// </summary>
        /// <value>The property observations.</value>
        [JsonPropertyName("observations")]
        public List<VisFormatObservation> Observations { get; set; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        [JsonIgnore]
        public Property Property
        {
            get
            {
                return new Property
                (
                    Identity.Create(Id),
                    Observations.Select(observation => observation.Observation)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatProperty"/> class with the specified key and value.
        /// </summary>
        /// <param name="property">The property.</param>
        public VisFormatProperty(Property property)
        {
            Id = property.Identifier.ToString();
            Observations = property.Observations.Select(observation => new VisFormatObservation(observation)).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatProperty"/> class.
        /// </summary>
        public VisFormatProperty() { }
    }

    /// <summary>
    /// Represents an observation in Vis format.
    /// </summary>
    public class VisFormatObservation
    {
        /// <summary>
        /// Gets or sets the observation type.
        /// </summary>
        /// <value>
        /// The human-readable observation type.
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the observation value.
        /// </summary>
        /// <value>
        /// The observation value.
        /// </value>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets the observation.
        /// </summary>
        /// <value>The observation.</value>
        [JsonIgnore]
        public Observation Observation
        {
            get
            {
                return new Observation
                {
                    Type = Type,
                    Value = Value
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatObservation"/> class with the specified observation.
        /// </summary>
        /// <param name="observation">The observation.</param>
        public VisFormatObservation(Observation observation)
        {
            Type = observation.Type;
            Value = observation.Value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatObservation"/> class.
        /// </summary>
        public VisFormatObservation() { }
    }
}