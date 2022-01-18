using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Operations.Hyperthought.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Hyperthought
{
    /// <summary>
    /// The input for the <see cref="UpdateHyperthoughtProcessOperation" /> operation.
    /// </summary>
    public struct UpdateHyperthoughtProcessOperationIn
    {
        // TODO: Implement authentication attribute to automatically fill this field in.
        /// <summary>
        /// The reference to the authenticated HyperThought API.
        /// </summary>
        [OperationAuthentication("hyperthought")]
        public HyperthoughtApi Api { get; set; }

        /// <summary>
        /// The full path of the process, e.g. /source/resource/workflow/builds/part/a1/results.
        /// </summary>
        public string ProcessPath { get; set; }

        /// <summary>
        /// The process path seperator. Defaults to '.'.
        /// </summary>
        public string PathSeperator { get; set; }

        /// <summary>
        /// A list of properties to add/update the HyperThought process with.
        /// </summary>
        public List<HyperthoughtProperty> Properties { get; set; }

        /// <summary>
        /// Overwrite the value if a property with the given key already exists. Defaults to true. If set to false,
        /// and any of the properties in the input list  already exists, the operation will not be performed, and
        /// the operation will exit with an error message.
        /// </summary>
        public bool? OverwriteExisting { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="UpdateHyperthoughtProcessOperation" /> operation.
    /// </summary>
    public struct UpdateHyperthoughtProcessOperationOut { }

    /// <summary>
    /// Updates properties on an existing Hyperthought process vertex, or creates a new HyperThought process vertex
    /// (with or without properties).
    /// </summary>
    [OperationName(Display = "Update HyperThought Process", Type = "hyperthoughtUpdateProcess")]
    [OperationTag(OperationTags.Hyperthought)]
    [OperationTag(OperationTags.Saving)]
    public class UpdateHyperthoughtProcessOperation : TypedOperation
    <
        UpdateHyperthoughtProcessOperationIn,
        UpdateHyperthoughtProcessOperationOut
    >
    {
        private const string KeyUnit = "unit";

        /// <summary>
        /// Constructs a <see cref="HyperthoughtMetadataValue"/> object given a property value.
        /// </summary>
        /// <param name="property">The property to be converted to a Hyperthouhgt Metadata Value</param>
        /// <returns>
        /// A list of <see cref="HyperthoughtMetadata"/> objects.
        /// </returns>
        private static HyperthoughtMetadataValue GetHyperthoughtMetaDataValue(HyperthoughtProperty property)
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
        private static List<HyperthoughtMetadata> UpdateHyperthoughtMetaData(
            List<HyperthoughtProperty> properties,
            HyperthoughtProcess process)
        {
            // Initialize local variables.
            List<HyperthoughtMetadata> metadataList = new();
            List<string> updatedPropertyNames = new();

            // Return if the properties or process does not exist.
            if ((properties is null) | (process is null)) return metadataList;

            // Update existing properties from the Hyperthought process.
            if (process.Metadata is not null)
            {
                foreach (HyperthoughtMetadata metadata in process.Metadata)
                {
                    if (properties.Any(m => m.Key == metadata.Key))
                    {
                        updatedPropertyNames.Add(metadata.Key);
                        HyperthoughtProperty property = properties.First(m => m.Key == metadata.Key);
                        if ((property.Value is not null) && (!string.IsNullOrWhiteSpace(property.Value.ToString())))
                            metadata.Value = GetHyperthoughtMetaDataValue(property);
                        if (!string.IsNullOrWhiteSpace(property.Annotation))
                            metadata.Annotation = property.Annotation;
                        if (!string.IsNullOrWhiteSpace(property.Unit))
                        {
                            Dictionary<string, object> metadataExtensions;
                            if (metadata.Extensions is not null)
                            {
                                metadataExtensions = metadata.Extensions;
                                if (metadataExtensions.ContainsKey(KeyUnit))
                                    metadataExtensions[KeyUnit] = property.Unit;
                                else
                                    metadataExtensions.Add(KeyUnit, property.Unit);
                            }
                            else
                            {
                                metadataExtensions = new();
                                metadataExtensions.Add(KeyUnit, property.Unit);
                            }
                            metadata.Extensions = metadataExtensions;
                        }
                    }
                    metadataList.Add(metadata);
                }
            }

            // Process new properties.
            foreach (HyperthoughtProperty property in properties)
            {
                if (!updatedPropertyNames.Contains(property.Key))
                {
                    HyperthoughtMetadata metadata = new()
                    {
                        Key = property.Key,
                        Value = GetHyperthoughtMetaDataValue(property)
                    };
                    if (!string.IsNullOrWhiteSpace(property.Annotation))
                        metadata.Annotation = property.Annotation;
                    if (!string.IsNullOrWhiteSpace(property.Unit))
                    {
                        metadata.Extensions = new();
                        metadata.Extensions.Add(KeyUnit, property.Unit);
                    }
                    metadataList.Add(metadata);
                }

            }
            return metadataList;
        }

        /// <summary>
        /// Updates a Hyperthought vertex with Carta properties.
        /// </summary>
        /// <param name="properties">A list of properties.</param>
        /// <param name="overwriteExisting">True if existing property values should be overwritten, else false.</param>
        /// <param name="process">The Hyperthought process to be updated.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
        private static async Task PatchHyperthoughtProcessVertex(
            List<HyperthoughtProperty> properties,
            bool overwriteExisting,
            HyperthoughtProcess process,
            HyperthoughtApi api)
        {
            Console.WriteLine($"Updating properties of process {process.Content.Name}... ");
            if (overwriteExisting)
            {
                List<string> processKeys = process.Metadata.Select(item => item.Key).ToList();
                List<string> propertyKeys = properties.Select(item => item.Key).ToList();
                IEnumerable<string> sharedKeys = processKeys.Intersect(propertyKeys);
                if (sharedKeys.Any())
                {
                    throw new ArgumentException($"The following property keys already exist under the process: " +
                        $" {string.Join(";", sharedKeys)} and OverwriteExisting is set to {overwriteExisting}." +
                        $" The process will therefore not be updated.");
                }
            }
            process.Metadata = UpdateHyperthoughtMetaData(properties, process);
            await api.Workflow.UpdateProcessAsync(process);

            // TODO: Add support for logging within operations.
            Console.WriteLine("Updated the process");
        }

        /// <inheritdoc />
        public override async Task<UpdateHyperthoughtProcessOperationOut> Perform(UpdateHyperthoughtProcessOperationIn input)
        {
            // Set default input values.
            input.PathSeperator ??= ".";
            input.OverwriteExisting ??= true;

            // Hyperthought does not want the path to start with a seperator - strip that off if need be.
            if (input.ProcessPath.StartsWith(input.PathSeperator))
                input.ProcessPath = input.ProcessPath[1..];

            // Get the process object.
            HyperthoughtProcess process =
                await input.Api.Workflow.GetProcessFromPathAsync(input.ProcessPath, input.PathSeperator);
            if (process is null)
            {
                throw new ArgumentException($"A process with path {input.ProcessPath} and path seperator " +
                    $"'{input.PathSeperator}' does not exist in HyperThought", input.ProcessPath);
            }

            // Update the process with the given properties.
            await PatchHyperthoughtProcessVertex(input.Properties, input.OverwriteExisting.Value, process, input.Api);

            return new UpdateHyperthoughtProcessOperationOut();
        }

    }
}
