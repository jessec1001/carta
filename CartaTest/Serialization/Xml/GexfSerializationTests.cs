using System.IO;
using System.Xml;
using System.Xml.Serialization;

using QuikGraph;
using NUnit.Framework;

using CartaCore.Data;
using CartaWeb.Serialization.Xml;

namespace CartaTest.Serialization.Xml
{
    /*
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Tests the serialization of a freeform graph into Graph Exchange XML format.
    /// </summary>
    [TestFixture]
    public class GexfSerializationTests
    {
        /// <summary>
        /// Tests the serialization and deserialization of a simple undirected graph.
        /// </summary>
        [Test]
        public void TestGexfReserialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GexFormat));
            GexFormat sample = new GexFormat(GraphHelpers.UndirectedGraphSample);

            string str;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(xmlWriter, sample);
                }
                str = stringWriter.ToString();
            }

            GexFormat data;
            using (StringReader stringReader = new StringReader(str))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    data = (GexFormat)serializer.Deserialize(xmlReader);
                }
            }

            FreeformGraph graph = data.Graph;

            Assert.NotNull(graph);
            Assert.AreEqual(5, graph.VertexCount);
            Assert.AreEqual(5, graph.EdgeCount);
        }
    }
    */
}