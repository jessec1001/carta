using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;
using CsvHelper;
using CsvHelper.Configuration;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ParseCsvOperation" /> operation.
    /// </summary>
    public struct ParseCsvOperationIn
    {
        /// <summary>
        /// The stream of data to retrieve the CSV from.
        /// </summary>
        [FieldName("Stream")]
        public Stream Stream { get; set; }

        /// <summary>
        /// The delimiter for the CSV file.
        /// </summary>
        [FieldRequired]
        [FieldDefault(",")]
        [FieldName("Delimiter")]
        public string Delimiter { get; set; }
        /// <summary>
        /// Whether the CSV file contains a header row.
        /// </summary>
        [FieldDefault(true)]
        [FieldName("Contains Header")]
        public bool ContainsHeader { get; set; }

        /// <summary>
        /// Whether the parser should infer the types of entries.
        /// If not true, all entries will be interpreted as text values.
        /// </summary>
        [FieldDefault(true)]
        [FieldName("Infer Types")]
        public bool InferTypes { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ParseCsvOperation" /> operation.
    /// </summary>
    public struct ParseCsvOperationOut
    {
        /// <summary>
        /// The graph dataset parsed from the CSV file.
        /// </summary>
        [FieldName("Graph")]
        public MemoryGraph Graph { get; set; }
    }

    /// <summary>
    /// Reads a data stream to be interpretted as a CSV-formatted file and converts it into a graph dataset.
    /// </summary>
    [OperationName(Display = "Parse CSV", Type = "parseCsv")]
    [OperationTag(OperationTags.Parsing)]
    public class ParseCsvOperation : TypedOperation
    <
        ParseCsvOperationIn,
        ParseCsvOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<ParseCsvOperationOut> Perform(ParseCsvOperationIn input)
        {
            // Get the default delimiter if it is not specified.
            string delimiter = input.Delimiter ?? ",";

            // Setup the CSV for parsing using the CSV helper library. 
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                DetectColumnCountChanges = true,
                HasHeaderRecord = input.ContainsHeader,
                TrimOptions = TrimOptions.Trim,
            };
            using StreamReader streamReader = new(input.Stream);
            using CsvReader csvReader = new(streamReader, csvConfig);

            // Notice that we need to construct the graph in memory because we have a possibly read-only stream. 
            // Construct the graph structure by reading all of the samples/rows.
            MemoryGraph graph = new(nameof(ParseCsvOperation));

            // If the input should contain a header, try to read it.
            if (input.ContainsHeader)
            {
                await csvReader.ReadAsync();
                csvReader.ReadHeader();
            }

            // Read each sample individually.
            int vertexCount = 0;
            while (await csvReader.ReadAsync())
            {
                // Read each field as a property.
                Dictionary<string, IProperty> properties = new();
                for (int index = 0; index < csvReader.ColumnCount; index++)
                {
                    // Read the field value; if we should infer types, do that here.
                    string fieldValue = csvReader.GetField(index);
                    string propertyName = input.ContainsHeader
                        ? csvReader.HeaderRecord[index]
                        : index.ToString();
                    object propertyValue = input.InferTypes
                        ? InferTypedValue(fieldValue)
                        : fieldValue;

                    // Set the property.
                    properties.Add(propertyName, new Property(propertyValue));
                }

                // Construct the vertex with its properties and add it to the graph.
                Vertex vertex = new(vertexCount++.ToString(), properties);
                graph.AddVertex(vertex);
            }

            return new ParseCsvOperationOut { Graph = graph };
        }

        /// <summary>
        /// Infers the type of a value by its string representation.
        /// </summary>
        /// <param name="field">The CSV field entry.</param>
        /// <returns>The typed value of the field.</returns>
        private static object InferTypedValue(string field)
        {
            // Try to convert to null.
            if (string.IsNullOrEmpty(field))
                return null;

            // Try to convert to boolean.
            if (field == "true")
                return true;
            if (field == "false")
                return false;

            // Try to convert to number.
            if (int.TryParse(field, out int intValue))
                return intValue;
            if (double.TryParse(field, out double doubleValue))
                return doubleValue;

            // Convert to string.
            return field;
        }
    }
}