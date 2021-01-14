using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using CartaCore.Serialization.Xml;
using QuikGraph;
using NUnit.Framework;

namespace CartaTest.Serialization.Xml
{
    [TestFixture]
    public class GexfSerializationTests
    {
        protected IUndirectedGraph<int, Edge<int>> TestGraph;

        [SetUp]
        public void SetUp()
        {
            // Create our test graph.
            IList<Edge<int>> edgeList = new Edge<int>[] {
                new Edge<int>(0, 1),
                new Edge<int>(0, 2),
                new Edge<int>(1, 3),
                new Edge<int>(2, 3),
                new Edge<int>(3, 4)
            };
            TestGraph = edgeList.ToUndirectedGraph<int, Edge<int>>();
        }

        [Test]
        public void TestGexfSerialize()
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    Gexf graph = new Gexf(TestGraph);
                    XmlSerializer serializer = new XmlSerializer(typeof(Gexf));

                    serializer.Serialize(xmlWriter, graph);
                }
                string str = stringWriter.ToString();
            }

            Assert.Pass();
        }

        [Test]
        public void TestGexfDeserialize()
        {
            string gexfString =
            @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <gexf xmlns=""http://www.gexf.net/1.2draft"" version=""1.2"">
                <meta lastmodifieddate=""2009-03-20"">
                    <creator>Gexf.net</creator>
                    <description>A hello world! file</description>
                </meta>
                <graph mode=""static"" defaultedgetype=""directed"">
                    <nodes>
                        <node id=""0"" label=""Hello"" />
                        <node id=""1"" label=""Word"" />
                    </nodes>
                    <edges>
                        <edge id=""0"" source=""0"" target=""1"" />
                    </edges>
                </graph>
            </gexf>
            ";

            IUndirectedGraph<int, Edge<int>> graph;
            using (StringReader stringReader = new StringReader(gexfString))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Gexf));
                    Gexf gexf = (Gexf)serializer.Deserialize(xmlReader);
                    graph = gexf.GraphValue;
                }
            }

            Assert.AreEqual(2, graph.VertexCount);
            Assert.AreEqual(1, graph.EdgeCount);
            foreach (int vertex in new int[] { 0, 1 })
                Assert.IsTrue(
                    graph.Vertices.Contains(vertex)
                );
            foreach (Edge<int> edge in new Edge<int>[] { new Edge<int>(0, 1) })
                Assert.AreEqual(1,
                    graph.Edges
                    .Where(
                        other =>
                            edge.Source == other.Source &&
                            edge.Target == other.Target
                    )
                    .Count()
                );
        }

        [Test]
        public void TestGexfReserialize()
        {
            string gexfString;
            IUndirectedGraph<int, Edge<int>> gexfGraph;

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    Gexf graph = new Gexf(TestGraph);
                    XmlSerializer serializer = new XmlSerializer(typeof(Gexf));

                    serializer.Serialize(xmlWriter, graph);
                }
                gexfString = stringWriter.ToString();
            }
            using (StringReader stringReader = new StringReader(gexfString))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Gexf));
                    gexfGraph = ((Gexf)serializer.Deserialize(xmlReader)).GraphValue;
                }
            }

            Assert.AreEqual(TestGraph.VertexCount, gexfGraph.VertexCount);
            Assert.AreEqual(TestGraph.EdgeCount, gexfGraph.EdgeCount);
        }
    }
}