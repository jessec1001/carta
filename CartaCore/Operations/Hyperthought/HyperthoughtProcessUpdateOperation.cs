using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Operations.Hyperthought.Data;

namespace CartaCore.Operations.Hyperthought
{
    /// <summary>
    /// The input for the <see cref="HyperthoughtProcessUpdateOperation" /> operation.
    /// </summary>
    public struct InputHyperthoughtProcesssUpdateOperation
    {
        /// <summary>
        /// The full path of the process, e.g. /source/resource/workflow/builds/part/a1/results.
        /// </summary>
        public string ProcessPath;

        /// <summary>
        /// The process path seperator. Defaults to '.'
        /// </summary>
        public string PathSeperator;

        /// <summary>
        /// A list of properties to add/update the Hyperthought process with.
        /// </summary>
        public List<HyperthoughtProperty> Properties;

        /// <summary>
        /// Overwrite the value if a property with the given key already exists. Defaults to true. If set to false,
        /// and any of the properties in the input list  already exists, the operation will not be performed, and
        /// the operation will exit with an error message.
        /// </summary>
        public bool? OverwriteExisting;
    }

    /// <summary>
    /// Operation to update properties on an existing Hyperthought process vertex,
    /// or to create a new Hyperthought process vertex (with or without properties)
    /// </summary>
    public class HyperthoughtProcessUpdateOperation
    {
        private HyperthoughtWorkflowApi WorkflowApi;

        /// <summary>
        /// Creates a new instance of the <see cref="HyperthoughtProcessUpdateOperation"/> class 
        /// </summary>
        /// <param name="api">An instance of the <see cref="HyperthoughtApi"/> class, with the necessary
        /// Hyperthought API credentials</param>
        public HyperthoughtProcessUpdateOperation(HyperthoughtApi api)
        {
            WorkflowApi = new HyperthoughtWorkflowApi(api);
        }

        /// <summary>
        /// Constructs a <see cref="HyperthoughtMetadataValue"/> object given a property value.
        /// </summary>
        /// <param name="property">The property to be converted to a Hyperthouhgt Metadata Value</param>
        /// <returns>
        /// A list of <see cref="HyperthoughtMetadata"/> objects.
        /// </returns>
        public static HyperthoughtMetadataValue GetHyperthoughtMetaDataValue(HyperthoughtProperty property)
        {
            return new HyperthoughtMetadataValue
            {
                Link = property.Value,
                Type = HyperthoughtDataType.String      
            };
        }

        /// <summary>
        /// Returns a list of <see cref="HyperthoughtMetadata"/> objects, updated with properties set on a vertex. 
        /// </summary>
        /// <param name="properties">Properties that should be updated/added to the Hyperthought process.</param>
        /// <param name="process">The Hyperthought process to be updated.</param>
        /// <returns>
        /// A list of <see cref="HyperthoughtMetadata"/> objects.
        /// </returns>
        public static List<HyperthoughtMetadata> UpdateHyperthoughtMetaData(
            List<HyperthoughtProperty> properties,
            HyperthoughtProcess process)
        {
            // Initialize local variables
            List<HyperthoughtMetadata> metadataList = new();
            List<string> updatedPropertyNames = new();

            // Return if the properties or process does not exist
            if ((properties is null) | (process is null)) return metadataList;

            // Update existing properties from the Hyperthought process
            if (process.Metadata is not null)
            {
                foreach (HyperthoughtMetadata metadata in process.Metadata)
                {
                    if (properties.Any(m => m.Key == metadata.Key))
                    {
                        updatedPropertyNames.Add(metadata.Key);
                        HyperthoughtProperty property = properties.First(m => m.Key == metadata.Key);
                        if ((property.Value is not null) && (!String.IsNullOrWhiteSpace(property.Value.ToString())))
                            metadata.Value = GetHyperthoughtMetaDataValue(property);
                        if (!String.IsNullOrWhiteSpace(property.Annotation))
                            metadata.Annotation = property.Annotation;
                        if (!String.IsNullOrWhiteSpace(property.Unit))
                        {
                            Dictionary<string, object> metadataExtensions;
                            if (metadata.Extensions is not null)
                            {
                                metadataExtensions = metadata.Extensions;
                                if (metadataExtensions.ContainsKey("unit"))
                                    metadataExtensions["unit"] = property.Unit;
                                else
                                    metadataExtensions.Add("unit", property.Unit);
                            }
                            else
                            {
                                metadataExtensions = new();
                                metadataExtensions.Add("unit", property.Unit);
                            }
                            metadata.Extensions = metadataExtensions;
                        }
                    }
                    metadataList.Add(metadata);
                }
            }

            // Process new properties 
            foreach (HyperthoughtProperty property in properties)
            {
                if (!updatedPropertyNames.Contains(property.Key))
                {
                    HyperthoughtMetadata metadata = new HyperthoughtMetadata();
                    metadata.Key = property.Key;
                    metadata.Value = GetHyperthoughtMetaDataValue(property);
                    if (!String.IsNullOrWhiteSpace(property.Annotation))
                        metadata.Annotation = property.Annotation;
                    if (!String.IsNullOrWhiteSpace(property.Unit))
                    {
                        metadata.Extensions = new();
                        metadata.Extensions.Add("unit", property.Unit);
                    }
                    metadataList.Add(metadata);
                }

            }
            return metadataList;
        }

        /// <summary>
        /// Updates a Hyperthought vertex with Carta properties
        /// </summary>
        /// <param name="properties">A list of properties.</param>
        /// <param name="overwriteExisting">True if existing property values should be overwritten, else false.</param>
        /// <param name="process">The Hyperthought process to be updated.</param>
        public async Task PatchHyperthoughtProcessVertex(
            List<HyperthoughtProperty> properties,
            bool? overwriteExisting,
            HyperthoughtProcess process)
        {
            Console.WriteLine($"Updating properties of process {process.Content.Name}... ");
            if ((overwriteExisting.HasValue) && (overwriteExisting.Value == false))
            {
                List<string> processKeys = process.Metadata.Select(item => item.Key).ToList();
                List<string> propertyKeys = properties.Select(item => item.Key).ToList();
                IEnumerable<string> sharedKeys = processKeys.Intersect(propertyKeys);
                if (sharedKeys.Count() > 0)
                {
                    throw new ArgumentException($"The following property keys already exist under the process: " +
                        $" {String.Join(";", sharedKeys)} and OverwriteExisting is set to {overwriteExisting.Value}." +
                        $" The process will therefore not be updated.");
                }
            }
            process.Metadata = UpdateHyperthoughtMetaData(properties, process);
            await WorkflowApi.UpdateProcessAsync(process);
            Console.WriteLine("Updated the process");
        }

        /// <summary>
        /// Perform the operation
        /// </summary>
        /// <param name="input">An <see cref="InputHyperthoughtProcesssUpdateOperation" /> instance.
        /// </param>
        public async Task Perform(
            InputHyperthoughtProcesssUpdateOperation input)
        {
            // Set default path seperator
            if (input.PathSeperator is null) input.PathSeperator = ".";

            // Hyperthought does NOT want the path to start with a seperator - strip that off if need be
            if (input.ProcessPath.StartsWith(input.PathSeperator))
                input.ProcessPath = input.ProcessPath.Substring(1);

            // Get the process object
            HyperthoughtProcess
                process = await WorkflowApi.GetProcessFromPathAsync(input.ProcessPath, input.PathSeperator);
            if (process is null)
                throw new ArgumentException($"A process with path {input.ProcessPath} and path seperator " +
                    $"'{input.PathSeperator}' does not exist in HyperThought", input.ProcessPath);
                
            // Update the process with the given properties
            await PatchHyperthoughtProcessVertex(input.Properties, input.OverwriteExisting, process);
        }

    }
}
