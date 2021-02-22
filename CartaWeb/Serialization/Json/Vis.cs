using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using CartaCore.Data.Freeform;

namespace CartaWeb.Serialization.Json
{
    /// <summary>
    /// Represents a freeform graph in Vis format.
    /// </summary>
    /// <remarks>
    /// This class allows for easy serialization and deserialization to and from JSON. This format allows the
    /// serialized data to immediately be used by Vis.js.
    /// </remarks>
    public class VisFormat
    {
        /// <summary>
        /// Gets or sets whether the graph is directed.
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
        /// Gets the freeform graph.
        /// </summary>
        /// <value>
        /// The freeform graph.
        /// </value>
        [JsonIgnore]
        public FreeformGraph Graph
        {
            get
            {
                // Create a graph and add the vertices and edges.
                FreeformFiniteGraph graph = new FreeformFiniteGraph(Directed);
                graph.AddVertexRange(Nodes.Select(node => node.Vertex));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormat"/> class with the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to a new format.</param>
        public VisFormat(FreeformGraph graph)
        {
            Directed = graph.IsDirected;

            Nodes = graph.Vertices.Select(node => new VisFormatNode(node)).ToList();
            Edges = graph.Edges.Select(edge => new VisFormatEdge(edge)).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormat"/> class.
        /// </summary>
        public VisFormat() { }
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
        /// Gets the freeform vertex.
        /// </summary>
        /// <value>
        /// The freeform vertex.
        /// </value>
        [JsonIgnore]
        public FreeformVertex Vertex
        {
            get
            {
                return new FreeformVertex(FreeformIdentity.Create(Id))
                {
                    Label = Label,
                    Description = Description,
                    Properties = Properties?.Select(property => property.Property)
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatNode"/> class with the specified node.
        /// </summary>
        /// <param name="node">The vertex to convert to a new format.</param>
        public VisFormatNode(FreeformVertex node)
        {
            Id = node.Identifier.ToString();
            Label = node.Label;
            Description = string.IsNullOrEmpty(node.Description) ? null : node.Description;

            if (!(node.Properties is null))
                Properties = node.Properties.Select(property => new VisFormatProperty(property)).ToList();
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
        /// Gets the freeform edge.
        /// </summary>
        /// <value>
        /// The freeform edge.
        /// </value>
        [JsonIgnore]
        public FreeformEdge Edge
        {
            get
            {
                return new FreeformEdge
                (
                    FreeformIdentity.Create(Source),
                    FreeformIdentity.Create(Target),
                    FreeformIdentity.Create(Id)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatEdge"/> class with the specified ID and endpoints.
        /// </summary>
        /// <param name="edge">The edge source and target to convert to a new format.</param>
        public VisFormatEdge(FreeformEdge edge)
        {
            Id = edge.Identifier.ToString();

            Source = edge.Source.Identifier.ToString();
            Target = edge.Target.Identifier.ToString();
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
        /// Gets the freeform property.
        /// </summary>
        /// <value>
        /// The freeform property.
        /// </value>
        [JsonIgnore]
        public FreeformProperty Property
        {
            get
            {
                return new FreeformProperty(FreeformIdentity.Create(Id))
                {
                    Observations = Observations.Select(observation => observation.Observation)
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatProperty"/> class with the specified key and value.
        /// </summary>
        /// <param name="property">The property.</param>
        public VisFormatProperty(FreeformProperty property)
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
    /// Represents a freeform observation in Vis format.
    /// </summary>
    public class VisFormatObservation
    {
        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        /// <value>
        /// The human-readable property type.
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets the freeform observation.
        /// </summary>
        /// <value>The freeform observation.</value>
        [JsonIgnore]
        public FreeformObservation Observation
        {
            get
            {
                return new FreeformObservation
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
        public VisFormatObservation(FreeformObservation observation)
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