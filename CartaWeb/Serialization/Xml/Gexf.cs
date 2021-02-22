using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using QuikGraph;

using CartaCore.Data.Freeform;
using CartaCore.Utility;

namespace CartaWeb.Serialization.Xml
{
    /*
    /// <summary>
    /// Represents a freeform graph with metadata in Graph Exchange XML format.
    /// </summary>
    [XmlRoot(ElementName = "gexf", Namespace = "http://www.gexf.net/1.2draft")]
    public class GexFormat
    {
        /// <summary>
        /// Gets or sets the graph data.
        /// </summary>
        /// <value>
        /// The graph data.
        /// </value>
        [XmlElement(ElementName = "graph")]
        public GexFormatGraph Data { get; set; }

        /// <summary>
        /// Gets the freeform graph.
        /// </summary>
        /// <value>
        /// The freeform graph.
        /// </value>
        [XmlIgnore]
        public FreeformGraph Graph
        {
            get => Data.Graph;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormat"/> class with the specified graph.
        /// </summary>
        /// <param name="graph">The graph to be converted to a new format.</param>
        public GexFormat(FreeformGraph graph)
        {
            Data = new GexFormatGraph(graph);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormat"/> class.
        /// </summary>
        public GexFormat() { }
    }

    /// <summary>
    /// Represents whether the graph is temporally static or dynamic in Graph Exchange XML format.
    /// </summary>
    public enum GexFormatMode
    {
        /// <summary>
        /// The graph is static.
        /// </summary>
        [XmlEnum(Name = "static")]
        Static
    }

    /// <summary>
    /// Represents the type of edge in Graph Exchange XML format.
    /// </summary>
    public enum GexFormatEdgeType
    {
        /// <summary>
        /// Edges are directed.
        /// </summary>
        [XmlEnum(Name = "directed")]
        Directed,
        /// <summary>
        /// Edges are undirected.
        /// </summary>
        [XmlEnum(Name = "undirected")]
        Undirected
    }

    /// <summary>
    /// Represents a freeform graph in Graph Exchange XML format.
    /// </summary>
    public class GexFormatGraph
    {
        /// <summary>
        /// Gets or sets the graph nodes.
        /// </summary>
        /// <value>
        /// The graph nodes.
        /// </value>
        [XmlArray(ElementName = "nodes")]
        [XmlArrayItem(ElementName = "node")]
        public List<GexFormatNode> Nodes { get; set; }
        /// <summary>
        /// Gets or sets the graph edges.
        /// </summary>
        /// <value>
        /// The graph edges.
        /// </value>
        [XmlArray(ElementName = "edges")]
        [XmlArrayItem(ElementName = "edge")]
        public List<GexFormatEdge> Edges { get; set; }

        /// <summary>
        /// The temporal mode of the graph.
        /// </summary>
        [XmlAttribute(AttributeName = "mode")]
        public GexFormatMode Mode { get; set; }
        /// <summary>
        /// The type of edge in the graph.
        /// </summary>
        [XmlAttribute(AttributeName = "defaultedgetype")]
        public GexFormatEdgeType EdgeType { get; set; }

        /// <summary>
        /// Gets or sets the property definitions.
        /// </summary>
        /// <value>
        /// The property definitions.
        /// </value>
        [XmlElement(ElementName = "attributes")]
        public GexFormatPropertyDefinitionList PropertyDefinitions { get; set; }

        /// <summary>
        /// Gets the freeform graph.
        /// </summary>
        /// <value>
        /// The freeform graph.
        /// </value>
        public FreeformGraph Graph
        {
            get
            {
                // Create a graph with the correct directed variant.
                FreeformGraph graph;
                if (EdgeType == GexFormatEdgeType.Directed)
                    graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>();
                else
                    graph = new UndirectedGraph<FreeformVertex, FreeformEdge>();

                // Get the property mapping.
                Dictionary<int, (string Name, FreeformProperty Property)> properties = PropertyDefinitions.Definitions
                    .ToDictionary
                    (
                        def => def.Id,
                        def => (def.Name,
                        new FreeformProperty
                        {
                            Type = def.Type.TypeDeserialize()
                        })
                    );

                // Add the vertices and edges.
                graph.AddVertexRange(Nodes.Select(node => new FreeformVertex(node.Id)
                {
                    Label = node.Label,
                    Description = node.Description,
                    Properties = new SortedList<string, FreeformProperty>
                    (
                        node.Properties.ToDictionary
                        (
                            prop => properties[prop.Id].Name,
                            prop =>
                            {
                                (string _, FreeformProperty property) = properties[prop.Id];
                                return new FreeformProperty
                                {
                                    Type = property.Type,
                                    Value = prop.Value
                                };
                            }
                        )
                    )
                }));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatGraph"/> class with the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to a new format.</param>
        public GexFormatGraph(FreeformGraph graph)
        {
            // We need the property mapping before creating the nodes.
            Dictionary<string, (int index, FreeformProperty prop)> properties =
                new Dictionary<string, (int index, FreeformProperty prop)>();
            foreach (FreeformVertex vertex in graph.Vertices)
            {
                foreach (KeyValuePair<string, FreeformProperty> property in vertex.Properties)
                {
                    if (!properties.ContainsKey(property.Key))
                    {
                        properties.Add
                        (
                            property.Key,
                            (
                                properties.Count,
                                property.Value
                            )
                        );
                    }
                }
            }
            Dictionary<string, int> propertyIds = properties.ToDictionary
            (
                pair => pair.Key,
                pair => pair.Value.index
            );

            Nodes = graph.Vertices.Select(vertex => new GexFormatNode(vertex, propertyIds)).ToList();
            Edges = graph.Edges.Select((edge, index) => new GexFormatEdge(index, edge)).ToList();

            Mode = GexFormatMode.Static;
            EdgeType = graph.IsDirected ? GexFormatEdgeType.Directed : GexFormatEdgeType.Undirected;

            PropertyDefinitions = new GexFormatPropertyDefinitionList(properties);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatGraph"/> class.
        /// </summary>
        public GexFormatGraph() { }
    }

    /// <summary>
    /// Represents a freeform vertex in Graph Exchange XML format.
    /// </summary>
    public class GexFormatNode
    {
        /// <summary>
        /// Gets or sets the vertex ID.
        /// </summary>
        /// <value>
        /// The vertex ID.
        /// </value>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vertex label.
        /// </summary>
        /// <value>
        /// The vertex label which is visible on vertex visualization.
        /// </value>
        [XmlAttribute(AttributeName = "label")]
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the vertex description.
        /// </summary>
        /// <value>
        /// The vertex description which pops up on vertex visualization.
        /// </value>
        [XmlAttribute(AttributeName = "title")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the vertex properties.
        /// </summary>
        /// <value>
        /// The vertex properties.
        /// </value>
        [XmlArray(ElementName = "attvalues")]
        [XmlArrayItem(ElementName = "attvalue")]
        public List<GexFormatProperty> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatNode"/> class with the specified vertex and
        /// and properties mapping.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        /// <param name="properties">The property mapping.</param>
        public GexFormatNode(FreeformVertex vertex, Dictionary<string, int> properties)
        {
            Id = vertex.Id;

            Label = vertex.Label;
            Description = vertex.Description;

            Properties = vertex.Properties.Select(pair =>
            {
                return new GexFormatProperty(properties[pair.Key], pair.Value);
            }).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatNode"/> class.
        /// </summary>
        public GexFormatNode() { }
    }

    /// <summary>
    /// Represents an freeform edge in Graph Exchange XML format.
    /// </summary>
    public class GexFormatEdge
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The edge ID.
        /// </value>
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the source vertex ID.
        /// </summary>
        /// <value>
        /// The source vertex ID.
        /// </value>
        [XmlAttribute(AttributeName = "source")]
        public Guid Source { get; set; }
        /// <summary>
        /// Gets or sets the target vertex ID.
        /// </summary>
        /// <value>
        /// The target vertex ID.
        /// </value>
        [XmlAttribute(AttributeName = "target")]
        public Guid Target { get; set; }

        /// <summary>
        /// Gets the freeform edge.
        /// </summary>
        /// <value>
        /// The freeform edge.
        /// </value>
        [XmlIgnore]
        public FreeformEdge Edge
        {
            get
            {
                return new FreeformEdge
                (
                    new FreeformVertex(Source),
                    new FreeformVertex(Target)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatEdge"/> class with the specified ID and endpoints.
        /// </summary>
        /// <param name="id">The edge ID.</param>
        /// <param name="edge">The edge source and target to convert to a new format.</param>
        public GexFormatEdge(int id, FreeformEdge edge)
        {
            Id = id;

            Source = edge.Source.Id;
            Target = edge.Target.Id;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatEdge"/> class.
        /// </summary>
        public GexFormatEdge() { }
    }

    /// <summary>
    /// Represents the class that an attribute belongs to in Graph Exchange XML format.
    /// </summary>
    public enum GexFormatClassType
    {
        /// <summary>
        /// The attribute belongs to a node.
        /// </summary>
        [XmlEnum(Name = "node")]
        Node,
        /// <summary>
        /// The attribute belongs to an edge.
        /// </summary>
        [XmlEnum(Name = "edge")]
        Edge
    }

    /// <summary>
    /// Represents definitions for properties in the Graph Exchange XML format.
    /// </summary>
    public class GexFormatPropertyDefinitionList
    {
        /// <summary>
        /// Gets or sets the properties class.
        /// </summary>
        /// <value>
        /// The class of the property definitions.
        /// </value>
        [XmlAttribute(AttributeName = "class")]
        public GexFormatClassType Class { get; set; }

        /// <summary>
        /// Gets or sets the property definitions.
        /// </summary>
        /// <value>
        /// The property definitions.
        /// </value>
        [XmlElement(ElementName = "attribute")]
        public List<GexFormatPropertyDefinition> Definitions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinitionList"/> class with the specified
        /// properties.
        /// </summary>
        /// <param name="properties">The properties to convert to a new format.</param>
        public GexFormatPropertyDefinitionList(Dictionary<string, (int index, FreeformProperty prop)> properties)
        {
            Class = GexFormatClassType.Node;
            Definitions = properties.Select
            (
                pair => new GexFormatPropertyDefinition
                (
                    pair.Value.index,
                    pair.Key,
                    pair.Value.prop
                )
            ).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinitionList"/> class.
        /// </summary>
        public GexFormatPropertyDefinitionList() { }
    }

    /// <summary>
    /// Represents the definition for a vertex property in Graph Exchange XML format.
    /// </summary>
    public class GexFormatPropertyDefinition
    {
        /// <summary>
        /// Gets or sets the property ID.
        /// </summary>
        /// <value>
        /// The property ID.
        /// </value>
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        [XmlAttribute(AttributeName = "title")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        /// <value>
        /// The property type.
        /// </value>
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinition"/> class with the specified ID,
        /// name, and property.
        /// </summary>
        /// <param name="id">The property ID.</param>
        /// <param name="name">The property name.</param>
        /// <param name="property">The property to convert to a new format.</param>
        public GexFormatPropertyDefinition(int id, string name, FreeformProperty property)
        {
            Id = id;

            Name = name;
            Type = property.Type.TypeSerialize();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinition"/> class.
        /// </summary>
        public GexFormatPropertyDefinition() { }
    }

    /// <summary>
    /// Represents a vertex property in Graph Exchange XML format.
    /// </summary>
    public class GexFormatProperty
    {
        /// <summary>
        /// Gets or sets the property ID.
        /// </summary>
        /// <value>
        /// The property ID. This must match an ID specified in a <see cref="GexFormatPropertyDefinition" />.
        /// </value>
        [XmlAttribute(AttributeName = "for")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatProperty"/> class with the specified ID and property.
        /// </summary>
        /// <param name="id">The property ID.</param>
        /// <param name="property">The property to convert to a new format.</param>
        public GexFormatProperty(int id, FreeformProperty property)
        {
            Id = id;

            Value = property.Value.ToString();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatProperty"/> class.
        /// </summary>
        public GexFormatProperty() { }
    }
    */
}