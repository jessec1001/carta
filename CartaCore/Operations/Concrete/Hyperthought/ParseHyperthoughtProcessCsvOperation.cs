using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CartaCore.Operations.Hyperthought.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Hyperthought
{

    /// <summary>
    /// The input for the <see cref="ParseHyperthoughtProcessCsvOperation" /> operation.
    /// </summary>
    public struct ParseHyperthoughtProcessCsvOperationIn
    {
        /// <summary>
        /// The stream of data to retrieve the CSV from.
        /// </summary>
        public Stream Stream { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ParseHyperthoughtProcessCsvOperation" /> operation.
    /// </summary>
    public struct ParseHyperthoughtProcessCsvOperationOut
    {
        /// <summary>
        /// A list of HyperThought process paths. 
        /// </summary>
        public string[] ProcessPaths { get; set; }
        /// <summary>
        /// A list of sets of HyperThought process properties corresponding to the process paths.
        /// </summary>
        public HyperthoughtProperty[][] ProcessProperties { get; set; }
    }

    /// <summary>
    /// Operation to parse a CSV stream and return a dictionary of properties to update a Hyperthought process with.
    /// </summary>
    [OperationName(Display = "Parse HyperThought Process CSV", Type = "hyperthoughtParseProcessCsv")]
    [OperationTag(OperationTags.Hyperthought)]
    [OperationTag(OperationTags.Parsing)]
    public class ParseHyperthoughtProcessCsvOperation : TypedOperation
    <
        ParseHyperthoughtProcessCsvOperationIn,
        ParseHyperthoughtProcessCsvOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<ParseHyperthoughtProcessCsvOperationOut> Perform(ParseHyperthoughtProcessCsvOperationIn input)
        {
            // Stores the list of process paths and properties in correspondence.
            List<string> processPaths = new();
            List<HyperthoughtProperty[]> processProperties = new();

            // Setup the CSV reader.
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                DetectColumnCountChanges = true,
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
            };
            StreamReader streamReader = new(input.Stream);
            CsvReader csvReader = new(streamReader, csvConfig);

            // Get the heading of the CSV.
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            string[] propertyHeader = csvReader.HeaderRecord;

            // Check that there is at least 1 property.
            if (csvReader.ColumnCount <= 1)
                throw new ArgumentException($"Invalid file content: file contains no properties");

            // Check if there is a second heading line - this means that there are sub-properties (units and annotation).
            string[] subpropertyHeader = null;
            await csvReader.ReadAsync();
            if ((csvReader.GetField(0) == "") & (csvReader.GetField(1) == ""))
            {
                subpropertyHeader = new string[csvReader.ColumnCount];
                for (int index = 0; index < csvReader.ColumnCount; index++)
                {
                    if ((csvReader.GetField(index) != "") & (propertyHeader[index] != ""))
                        throw new ArgumentException($"Invalid file content: column {index + 1} contains both a " +
                            $"property and sub-property name: property={propertyHeader[index]} and sub-property=" +
                            $"{csvReader.GetField(index)}");
                    subpropertyHeader[index] = csvReader.GetField(index);
                }
            }

            // Update the row count based on the number of heading lines we encountered.
            int rowCount = 1;
            if (subpropertyHeader is not null)
            {
                ++rowCount;
                await csvReader.ReadAsync();
            }

            // Read the remaining lines as process path and property lines.
            // The process path should be the first column, and the property values should be the remaining columns.
            do
            {
                // Setup a list of new properties for this process.
                ++rowCount;
                List<HyperthoughtProperty> properties = new();

                // Read the process path if possible.
                string processPath = csvReader.GetField(0);
                if (string.IsNullOrEmpty(processPath))
                    throw new ArgumentException($"Invalid file content: process path at line {rowCount} is empty");


                // If there are no units or annotations, then the properties can be read in simply.
                if (subpropertyHeader is null)
                {
                    for (int index = 1; index < csvReader.ColumnCount; index++)
                    {
                        string propertyName = propertyHeader[index];
                        if (string.IsNullOrEmpty(propertyName))
                            throw new ArgumentException($"Invalid file content: column {index + 1} has no property name");
                        object propertyValue = csvReader.GetField(index) == "" ? null : csvReader.GetField(index);
                        properties.Add(new HyperthoughtProperty { Key = propertyName, Value = propertyValue });
                    }
                }
                // If there are units or annotations, we need to take them into account when reading.
                else
                {
                    HyperthoughtProperty property = null;
                    for (int index = 1; index < csvReader.ColumnCount; index++)
                    {
                        // Read in the property name.
                        if (!string.IsNullOrEmpty(propertyHeader[index]))
                        {
                            // Add the previous property before creating a new property.
                            if (property is not null)
                                properties.Add(property);

                            string propertyName = propertyHeader[index];
                            string propertyValue = csvReader.GetField(index) == "" ? null : csvReader.GetField(index);
                            property = new HyperthoughtProperty { Key = propertyName, Value = propertyValue };
                        }

                        // Read in the property units and annotations.
                        if (!string.IsNullOrEmpty(subpropertyHeader[index]))
                        {
                            string subPropertyName = subpropertyHeader[index];
                            string propertyValue = csvReader.GetField(index) == "" ? null : csvReader.GetField(index);
                            if (subPropertyName == "units")
                                property.Unit = propertyValue;
                            if (subPropertyName == "annotation")
                                property.Annotation = propertyValue;
                        }
                    }

                    // Add the remaining property.
                    if (property is not null)
                        properties.Add(property);
                }

                // Append the process path and properties to the lists.
                processPaths.Add(processPath);
                processProperties.Add(properties.ToArray());
            }
            while (await csvReader.ReadAsync());

            return new ParseHyperthoughtProcessCsvOperationOut
            {
                ProcessPaths = processPaths.ToArray(),
                ProcessProperties = processProperties.ToArray(),
            };
        }

    }

}