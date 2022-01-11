using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests a workflow that loads data from a CSV file stored at a particular URL.
    /// </summary>
    public class TestParseCsvWorkflow
    {
        /// <summary>
        /// Tests that a CSV file containing information about the locations of cities can be read from a URL into a
        /// graph dataset.
        /// </summary>
        [Test]
        public async Task TestParseCsvCities()
        {
            // The following workflow (diagram) is generated by this test code.
            /*
                Input (Url) -> Url Stream -> Parse CSV -> Output (Graph)
            */
            // Equivalently, this workflow could be represented more standardly as:
            /*
                Workflow
                ++++++++++++++++++++++++++++++++++++++++++++++++++
                Operation #1 (Input)
                ["Url" ===========> Name                         ]
                [                               Value --------- A]

                Operation #2 (URL Stream)
                [A ---------------> Url                          ]
                [                               Stream -------- B]

                Operation #3 (CSV Parser)
                [B ---------------> Stream                       ]
                ["," =============> Delimiter                    ]
                [true ============> ContainsHeader               ]
                [true ============> InferTypes                   ]
                [                               Graph --------- C]

                Operation #4 (Output)
                [C ---------------> Value                        ]
                ["Graph" =========> Name                         ]
            */

            // Define the operations required.
            #region Operations
            InputOperation opInputUrl = new()
            {
                DefaultTyped = new() { Name = "Url" }
            };
            StreamUrlOperation opStreamUrl = new();
            ParseCsvOperation opParseCsv = new()
            {
                DefaultTyped = new()
                {
                    Delimiter = ",",
                    ContainsHeader = true,
                    InferTypes = true
                }
            };
            OutputOperation opOutputGraph = new()
            {
                DefaultTyped = new() { Name = "Graph" }
            };
            #endregion

            // Define the connections required.
            #region Connections
            WorkflowOperationConnection connUrlToStream = new()
            {
                Source = new() { Operation = opInputUrl.Identifier, Field = nameof(InputOperationOut.Value) },
                Target = new() { Operation = opStreamUrl.Identifier, Field = nameof(StreamUrlOperationIn.Url) }
            };
            WorkflowOperationConnection connStreamToCsv = new()
            {
                Source = new() { Operation = opStreamUrl.Identifier, Field = nameof(StreamUrlOperationOut.Stream) },
                Target = new() { Operation = opParseCsv.Identifier, Field = nameof(ParseCsvOperationIn.Stream) }
            };
            WorkflowOperationConnection connCsvToGraph = new()
            {
                Source = new() { Operation = opParseCsv.Identifier, Field = nameof(ParseCsvOperationOut.Graph) },
                Target = new() { Operation = opOutputGraph.Identifier, Field = nameof(OutputOperationIn.Value) }
            };
            #endregion

            // Create the workflow operation.
            WorkflowOperation opWorkflow = new(
                new Operation[]
                {
                    opInputUrl,
                    opStreamUrl,
                    opParseCsv,
                    opOutputGraph
                },
                new WorkflowOperationConnection[]
                {
                    connUrlToStream,
                    connStreamToCsv,
                    connCsvToGraph
                }
            );

            // Check that an input to the workflow is correctly processed and produces expected results.
            string csvUrl = "https://people.sc.fsu.edu/~jburkardt/data/csv/cities.csv";
            Dictionary<string, object> input = new()
            {
                ["Url"] = csvUrl
            };
            Dictionary<string, object> output = new();
            OperationContext context = new()
            {
                Input = input,
                Output = output
            };
            await opWorkflow.Perform(context);

            // Get the graph from the output.
            Assert.Contains("Graph", output.Keys);
            Assert.IsInstanceOf<FiniteGraph>(output["Graph"]);
            FiniteGraph graph = output["Graph"] as FiniteGraph;

            // Check that there is the correct number of samples.
            Assert.AreEqual(128, await graph.GetVertices().CountAsync());

            // Check that there are the correct properties and property types.
            await foreach (IVertex ivertex in graph.GetVertices())
            {
                // Assert that the vertex is of base type.
                Vertex vertex = ivertex as Vertex;
                Assert.IsNotNull(vertex);

                // Assert property names.
                string[] propertyNames = vertex.Properties
                    .Select(prop => prop.Identifier.ToString())
                    .ToArray();
                Assert.Contains("LatD", propertyNames); // Latitude Degrees
                Assert.Contains("LatM", propertyNames); // Latitude Minutes
                Assert.Contains("LatS", propertyNames); // Latitude Seconds
                Assert.Contains("NS", propertyNames);   // North/South
                Assert.Contains("LonD", propertyNames); // Longitude Degrees
                Assert.Contains("LonM", propertyNames); // Longitude Minutes
                Assert.Contains("LonS", propertyNames); // Longitude Seconds
                Assert.Contains("EW", propertyNames);   // East/West
                Assert.Contains("City", propertyNames);
                Assert.Contains("State", propertyNames);

                // Assert property types.
                // TODO: Make it easier to select a particular vertex from a graph.
                object latD = vertex.Properties.First(prop => prop.Identifier.Equals("LatD")).Value;
                object latM = vertex.Properties.First(prop => prop.Identifier.Equals("LatM")).Value;
                object latS = vertex.Properties.First(prop => prop.Identifier.Equals("LatS")).Value;
                object ns = vertex.Properties.First(prop => prop.Identifier.Equals("NS")).Value;
                object city = vertex.Properties.First(prop => prop.Identifier.Equals("City")).Value;
                object state = vertex.Properties.First(prop => prop.Identifier.Equals("State")).Value;
                Assert.IsInstanceOf<int>(latD);
                Assert.IsInstanceOf<int>(latM);
                Assert.IsInstanceOf<int>(latS);
                Assert.IsInstanceOf<string>(ns);
                Assert.IsInstanceOf<string>(city);
                Assert.IsInstanceOf<string>(state);
            }

            // Check some specific values.
            IVertex wichitaVertex = await graph
                .GetVertices()
                .FirstAsync(vertex =>
                    (string)vertex.Properties.First(prop => prop.Identifier.Equals("City")).Value == "Wichita"
                );
            Assert.AreEqual(37, (int)wichitaVertex.Properties.First(prop => prop.Identifier.Equals("LatD")).Value);
            Assert.AreEqual(97, (int)wichitaVertex.Properties.First(prop => prop.Identifier.Equals("LonD")).Value);
            Assert.AreEqual("KS", (string)wichitaVertex.Properties.First(prop => prop.Identifier.Equals("State")).Value);

            IVertex scrantonVertex = await graph
                .GetVertices()
                .FirstAsync(vertex =>
                    (string)vertex.Properties.First(prop => prop.Identifier.Equals("City")).Value == "Scranton"
                );
            Assert.AreEqual(41, (int)scrantonVertex.Properties.First(prop => prop.Identifier.Equals("LatD")).Value);
            Assert.AreEqual(75, (int)scrantonVertex.Properties.First(prop => prop.Identifier.Equals("LonD")).Value);
            Assert.AreEqual("PA", (string)scrantonVertex.Properties.First(prop => prop.Identifier.Equals("State")).Value);
        }
    }
}