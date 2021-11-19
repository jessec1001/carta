using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

using CsvHelper;
using CsvHelper.Configuration;

using CartaCore.Operations.Hyperthought.Data;

namespace CartaCore.Operations.Hyperthought
{

    /// <summary>
    /// The input for the <see cref="HyperthoughtProcessCsvParserOperation" /> operation.
    /// </summary>
    public struct InputHyperthoughtProcessCsvParserOperation
    {
        /// <summary>
        /// A CSV file stream 
        /// </summary>
        public Stream Stream;
    }

    /// <summary>
    /// The output for the <see cref="HyperthoughtProcessCsvParserOperation" /> operation.
    /// </summary>
    public struct OutputHyperthoughtProcessCsvParserOperation
    {
        /// <summary>
        /// A dictionary of properties to add/update the Hyperthought process with.
        /// Key = the Hyperthought process path, Value = a list of properties. 
        /// </summary>
        public Dictionary<string, List<HyperthoughtProperty>> PropertiesDictionary;
    }

    /// <summary>
    /// Operation to parse a CSV stream and return a dictionary of properties to update a Hyperthought process with.
    /// </summary>
    public class HyperthoughtProcessCsvParserOperation
    {
        private string[] PropertyHeader;
        private string[] SubPropertyHeader;

        /// <summary>
        /// Perform the operation
        /// </summary>
        /// <param name="input">An <see cref="InputHyperthoughtProcesssUpdateOperation" /> instance.
        /// </param>
        public async Task<OutputHyperthoughtProcessCsvParserOperation> Perform(
            InputHyperthoughtProcessCsvParserOperation input)
        {
            Dictionary<string, List<HyperthoughtProperty>> propertiesDictionary = new();

            // Get the header line
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                DetectColumnCountChanges = true,
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
            };
            StreamReader streamReader = new(input.Stream);
            CsvReader csvReader = new(streamReader, csvConfig);
            await csvReader.ReadAsync();
            csvReader.ReadHeader();
            PropertyHeader = csvReader.HeaderRecord;

            // Check that there is at least 1 property
            if (csvReader.ColumnCount < 2)
                throw new ArgumentException($"Invalid file content: file contains no properties");

            // Check if there is a second header line - this means that there are sub-properties (units and annotation)
            await csvReader.ReadAsync();
            if ((csvReader.GetField(0) == "") & (csvReader.GetField(1) == ""))
            {
                SubPropertyHeader = new string[csvReader.ColumnCount];
                for (int index = 0; index < csvReader.ColumnCount; index++)
                {
                    if ((csvReader.GetField(index) != "") & (PropertyHeader[index] != ""))
                        throw new ArgumentException($"Invalid file content: column {index+1} contains both a " +
                            $"property and sub-property name: property={PropertyHeader[index]} and sub-property=" +
                            $"{csvReader.GetField(index)}");
                    SubPropertyHeader[index] = csvReader.GetField(index);
                }
            }

            int rowCount = 1;
            if (SubPropertyHeader is not null)
            {
                rowCount = 2;
                await csvReader.ReadAsync();
            }
                
            do
            {
                ++rowCount;
                List<HyperthoughtProperty> properties = new();

                string processPath = csvReader.GetField(0);
                if (processPath == "")
                    throw new ArgumentException($"Invalid file content: process path at line {rowCount} is empty");

                if (SubPropertyHeader is null)
                {
                    for (int index = 1; index < csvReader.ColumnCount; index++)
                    {
                        string propertyName = PropertyHeader[index];
                        if (propertyName == "")
                            throw new ArgumentException($"Invalid file content: column {index+1} has no property name");
                        object propertyValue = csvReader.GetField(index)=="" ? null : csvReader.GetField(index);
                        properties.Add(new HyperthoughtProperty { Key = propertyName, Value = propertyValue});
                    }
                }
                else
                {
                    HyperthoughtProperty property=null;
                    for (int index = 1; index < csvReader.ColumnCount; index++)
                    {
                        if (PropertyHeader[index] != "")
                        {
                            if (property is not null)
                                properties.Add(property); // Add the previous property before creating a new property
                            string propertyName = PropertyHeader[index];
                            object propertyValue = csvReader.GetField(index)=="" ? null : csvReader.GetField(index);
                            property = new HyperthoughtProperty { Key = propertyName, Value = propertyValue };
                        }

                        if (SubPropertyHeader[index] != "")
                        {
                            string subPropertyName = SubPropertyHeader[index];                            
                            string propertyValue = csvReader.GetField(index) == "" ? null : csvReader.GetField(index);
                            if (subPropertyName == "units")
                                property.Unit = propertyValue;
                            if (subPropertyName == "annotation")
                                property.Annotation = propertyValue;
                        }
                    }
                    properties.Add(property); // Add the last property 
                }

                propertiesDictionary.Add(processPath, properties);
            }
            while (await csvReader.ReadAsync());

            return new OutputHyperthoughtProcessCsvParserOperation { PropertiesDictionary = propertiesDictionary};
        }

    }

}