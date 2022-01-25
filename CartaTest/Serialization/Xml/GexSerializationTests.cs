using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using CartaCore.Data;
using CartaWeb.Serialization.Xml;

namespace CartaTest.Serialization.Xml
{
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
        public async Task TestGexfReserialize()
        {
            // TODO: Redo this test to use XML object serialization.
            // XmlSerializer serializer = new XmlSerializer(typeof(GexFormat));
            // GexFormat sample = await GexFormat.CreateAsync(GraphHelpers.UndirectedGraphSample);

            // string str;
            // using (StringWriter stringWriter = new StringWriter())
            // {
            //     using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            //     {
            //         serializer.Serialize(xmlWriter, sample);
            //     }
            //     str = stringWriter.ToString();
            // }

            // GexFormat data;
            // using (StringReader stringReader = new StringReader(str))
            // {
            //     using (XmlReader xmlReader = XmlReader.Create(stringReader))
            //     {
            //         data = (GexFormat)serializer.Deserialize(xmlReader);
            //     }
            // }

            // IEntireGraph graph = data.Graph;

            // Assert.NotNull(graph);
            // Assert.AreEqual(5, await graph.GetVertices().CountAsync());
            // // Assert.AreEqual(5, await graph.GetEdges().CountAsync());
        }
    }
}