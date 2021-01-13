using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using QuikGraph;
using QuikGraph.Algorithms;

namespace CartaCore.Serialization.Xml
{
    [XmlRoot(ElementName = "gexf", Namespace = "http://www.gexf.net/1.2draft")]
    public class Gexf
    {
        /* To-do: metadata */

        [XmlElement(ElementName = "graph")]
        public GexfGraph Graph;

        [XmlIgnore]
        public IUndirectedGraph<int, Edge<int>> GraphValue
        {
            get
            {
                UndirectedGraph<int, Edge<int>> graph = new UndirectedGraph<int, Edge<int>>();
                graph.AddVertexRange(Graph.NodeValues);
                graph.AddEdgeRange(Graph.EdgeValues);
                return graph;
            }
        }

        public Gexf() { }
        public Gexf(IUndirectedGraph<int, Edge<int>> graph)
        {
            Graph = new GexfGraph(graph);
        }
    }

    public enum GexfMode
    {
        [XmlEnum(Name = "static")]
        Static
    }

    public enum GexfEdgeType
    {
        [XmlEnum(Name = "directed")]
        Directed,
        [XmlEnum(Name = "undirected")]
        Undirected
    }

    public class GexfGraph
    {
        [XmlArray(ElementName = "nodes")]
        [XmlArrayItem(ElementName = "node")]
        public List<GexfNode> Nodes;
        [XmlArray(ElementName = "edges")]
        [XmlArrayItem(ElementName = "edge")]
        public List<GexfEdge> Edges;

        [XmlIgnore]
        public IEnumerable<int> NodeValues => Nodes.Select(node => node.NodeValue);
        [XmlIgnore]
        public IEnumerable<Edge<int>> EdgeValues => Edges.Select(edge => edge.EdgeValue);

        [XmlAttribute(AttributeName = "mode")]
        public GexfMode Mode;
        [XmlAttribute(AttributeName = "defaultedgetype")]
        public GexfEdgeType EdgeType;

        public GexfGraph() { }
        public GexfGraph(IUndirectedGraph<int, Edge<int>> graph)
        {
            VertexIdentity<int> vi = graph.GetVertexIdentity();
            EdgeIdentity<int, Edge<int>> ei = graph.GetEdgeIdentity();

            Nodes = graph.Vertices.Select(vertex => new GexfNode(vi, ei, vertex)).ToList();
            Edges = graph.Edges.Select(edge => new GexfEdge(vi, ei, edge)).ToList();

            Mode = GexfMode.Static;
            EdgeType = GexfEdgeType.Undirected;
        }
    }

    public class GexfNode
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id;
        [XmlAttribute(AttributeName = "label")]
        public string Label;

        [XmlIgnore]
        public int NodeValue
        {
            get
            {
                int.TryParse(Id, out int result);
                return result;
            }
        }

        public GexfNode() { }
        public GexfNode(VertexIdentity<int> vertexId, EdgeIdentity<int, Edge<int>> edgeId, int vertex)
        {
            Id = Label = vertexId(vertex);
        }
    }

    public class GexfEdge
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id;
        [XmlAttribute(AttributeName = "source")]
        public string SourceId;
        [XmlAttribute(AttributeName = "target")]
        public string TargetId;

        [XmlIgnore]
        public Edge<int> EdgeValue
        {
            get
            {
                int.TryParse(SourceId, out int source);
                int.TryParse(TargetId, out int target);
                return new Edge<int>(source, target);
            }
        }

        public GexfEdge() { }
        public GexfEdge(VertexIdentity<int> vertexId, EdgeIdentity<int, Edge<int>> edgeId, Edge<int> edge)
        {
            Id = edgeId(edge);
            SourceId = vertexId(edge.Source);
            TargetId = vertexId(edge.Target);
        }
    }
}