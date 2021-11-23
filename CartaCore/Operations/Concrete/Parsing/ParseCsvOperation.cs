using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CartaCore.Data;
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
        public Stream Stream { get; set; }

        /// <summary>
        /// The delimiter for the CSV file.
        /// </summary>
        public string Delimiter { get; set; }
        /// <summary>
        /// Whether the CSV file contains a header row.
        /// </summary>
        public bool ContainsHeader { get; set; }

        /// <summary>
        /// Whether the parser should infer the types of entries.
        /// If not true, all entries will be interpreted as text values.
        /// </summary>
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
        public FiniteGraph Graph { get; set; }
    }

    /// <summary>
    /// Reads a data stream to be interpretted as a CSV-formatted file and converts it into a graph dataset.
    /// </summary>
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

            // Construct the graph structure by reading all of the samples/rows.
            FiniteGraph graph = new(Identity.Create($"{nameof(ParseCsvOperation)}"), true);

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
                List<Property> properties = new();
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
                    properties.Add(new Property(Identity.Create(propertyName), propertyValue));
                }

                // Construct the vertex with its properties and add it to the graph.
                Vertex vertex = new(Identity.Create(vertexCount++), properties.ToArray());
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