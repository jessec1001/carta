using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Operations.Attributes;
using CartaCore.Operations.Authentication;

namespace CartaCore.Operations.Hyperthought
{
    /// <summary>
    /// The input for the <see cref="UpdateHyperthoughtProcessFileLinkOperation" /> operation.
    /// </summary>
    public struct UpdateHyperthoughtProcessFileLineOperationIn
    {
        /// <summary>
        /// The reference to the authenticated HyperThought API.
        /// </summary>
        [FieldAuthentication(HyperthoughtAuthentication.Key, typeof(HyperthoughtAuthentication))]
        public HyperthoughtAuthentication HyperthoughtAuth { get; set; }

        /// <summary>
        /// The file path, e.g. /dirname/subdirname/filename.txt
        /// </summary>
        [FieldRequired]
        [FieldName("File Path")]
        public string FilePath { get; set; }

        /// <summary>
        /// The alias of the workspace the file is stored under. 
        /// </summary>
        [FieldRequired]
        [FieldName("Workspace Alias")]
        public string WorkspaceAlias { get; set; }

        /// <summary>
        /// The key (name) of the property the file should be linked to. 
        /// </summary>
        [FieldRequired]
        [FieldName("Property Key")]
        public string PropertyKey { get; set; }

        /// <summary>
        /// The full path of the process, e.g. /source/resource/workflow/builds/part/a1/results.
        /// </summary>
        [FieldRequired]
        [FieldName("Process Path")]
        public string ProcessPath { get; set; }

        /// <summary>
        /// The process path seperator.
        /// </summary>
        [FieldDefault(".")]
        [FieldName("Process Path Seperator")]
        public string PathSeperator { get; set; }

        /// <summary>
        /// Overwrite the value if the property with the given key already exists. Defaults to true. If set to false,
        /// and the property already exists, the operation will exit with a warning message.
        /// </summary>
        [FieldDefault(true)]
        [FieldName("Overwrite Existing")]
        public bool OverwriteExisting { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="UpdateHyperthoughtProcessFileLinkOperation" /> operation.
    /// </summary>
    public struct UpdateHyperthoughtProcessFileLineOperationOut { }

    /// <summary>
    /// Updates a file link property on an existing Hyperthought process vertex, or creates a new property.
    /// </summary>
    [OperationName(Display = "Update HyperThought Process File Link", Type = "hyperthoughtUpdateProcessFileLink")]
    [OperationTag(OperationTags.Hyperthought)]
    [OperationTag(OperationTags.Saving)]
    public class UpdateHyperthoughtProcessFileLinkOperation : TypedOperation
    <
        UpdateHyperthoughtProcessFileLineOperationIn,
        UpdateHyperthoughtProcessFileLineOperationOut
    >
    {
        /// <summary>
        /// Retrieves the workspace UUID of the workspace with the given alias. Note that Hyperthought requires unique
        /// aliases for workspaces, but allows for duplicate workspace names. For this reason, files are specified
        /// by workspace alias, rather than workspace name.
        /// </summary>
        /// <param name="workspaceAlias">The workspace alias.</param>
        /// <param name="api">The authenticated Hyperthought API.</param>
        private static async Task<Guid> GetWorkspaceId(string workspaceAlias, HyperthoughtApi api)
        {
            IList<HyperthoughtWorkspace> workspaces = await api.Workspaces.GetWorkspacesAsync();
            HyperthoughtWorkspace workspace = workspaces.FirstOrDefault(item => item.Alias == workspaceAlias);
            if (workspace is null) return Guid.Empty;
            else return workspace.PrimaryKey;
        }

        /// <summary>
        /// Obtains the HyperThought UUID directory path for a folder under the given UUID directory path and workspace
        /// ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs.</param>
        /// <param name="folderName">The name of the folder located under the given directory path.</param>
        /// <param name="workspaceId">The workspace ID of the folder.</param>
        /// <param name="api">The authenticated Hyperthought API.</param>
        /// <returns>A string of UUIDs representing a HyperThought API directory path.</returns>
        private static async Task<string> GetDirectoryIdPathAsync(
            string uuidPath,
            string folderName,
            Guid workspaceId,
            HyperthoughtApi api
        )
        {
            HyperthoughtFile hyperthoughtDir =
                await api.Files.GetFileAsync(uuidPath, folderName, workspaceId);
            if (hyperthoughtDir is null) return null;
            else return $"{uuidPath}{hyperthoughtDir.PrimaryKey},";
        }

        /// <summary>
        /// Obtains the HyperThought file link for a file under the given UUID directory path and workspace ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs.</param>
        /// <param name="fileName">The name of the file located under the given directory path.</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <param name="api">The authenticated Hyperthought API.</param>
        /// <returns>The file link; null is returned if the file does not exist.</returns>
        private static async Task<string> GetFileLinkAsync(
            string uuidPath,
            string fileName,
            Guid workspaceId,
            HyperthoughtApi api
        )
        {
            if (uuidPath is null) return null;
            HyperthoughtFile hyperthoughtFile = await api.Files.GetFileAsync(uuidPath, fileName, workspaceId);
            if (hyperthoughtFile is null)
                return null;
            // Note that the Hyperthought API does not have the file link returned in any of its response objects,
            // and versioning of files are not yet implemented.
            // From inspection of metadata stored under a Hyperthought Process node, the file link is set to the
            // file canonical URI, with "/versions/0.json" appended to it. This may change in future. 
            else
                return $"{hyperthoughtFile.CanonicalUri}/versions/0.json";
        }

        /// <summary>
        /// Gets the Hyperthought file link for a file given its name.
        /// </summary>
        /// <param name="filePath">The file name, including the file path, e.g. /dirname/subdirname/filename.txt</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <param name="api">The authenticated Hyperthought API.</param>
        /// <returns>The file link; null is returned if the file does not exist.</returns>
        private static async Task<string> GetFileLinkAysnc(string filePath, Guid workspaceId, HyperthoughtApi api)
        {
            // TODO: Add support for logging within operations.
            Console.WriteLine($"Getting the UUID for file {filePath}...");

            // First construct the UUID directory path
            string[] pathParts = filePath.Split(@"/");
            string fileName = pathParts[^1];
            string uuidPath = ",";
            for (int k = 1; k < (pathParts.Length - 1); k++)
            {
                Console.WriteLine($"...Getting UUIDPath for folder {pathParts[k]} under workspace UUID " +
                    $"{workspaceId} and UUIDPath {uuidPath}...");
                uuidPath = await GetDirectoryIdPathAsync(uuidPath, pathParts[k], workspaceId, api);
            }

            // Get and return the file link
            Console.WriteLine($"...Getting the link for file {fileName} under workspace UUID " +
                $"{workspaceId} and UUDIPath {uuidPath}...");
            string fileLink = await GetFileLinkAsync(uuidPath, fileName, workspaceId, api);
            Console.WriteLine($"File link for file {fileName} is {fileLink}");
            return fileLink;
        }

        /// <inheritdoc />
        public override async Task<UpdateHyperthoughtProcessFileLineOperationOut> Perform(UpdateHyperthoughtProcessFileLineOperationIn input)
        {
            // Set default input values.
            input.PathSeperator ??= ".";

            // Validate input values.
            HyperthoughtApi api = new(input.HyperthoughtAuth.ApiKey);
            Guid workspaceId = await GetWorkspaceId(input.WorkspaceAlias, api);
            if (workspaceId.Equals(Guid.Empty))
                throw new ArgumentException($"Workspace alias {input.WorkspaceAlias} does not exist.");
            if (input.FilePath is null)
                throw new ArgumentException($"File Path must be specified.");
            if (input.FilePath.Split(@"/").Length > 1)
            {
                if (!input.FilePath.StartsWith(@"/"))
                    throw new ArgumentException($"File Path does not start with '/'.");
            }

            // Get the file link.
            string fileLink = await GetFileLinkAysnc(input.FilePath, workspaceId, api);
            if (fileLink is null)
                throw new ArgumentException($"The file {input.FilePath} does not exist in HyperThought.");

            // Hyperthought does not want the path to start with a seperator - strip that off if need be.
            if (input.ProcessPath.StartsWith(input.PathSeperator))
                input.ProcessPath = input.ProcessPath[1..];

            // Get the process object.
            HyperthoughtProcess process =
                await api.Workflow.GetProcessFromPathAsync(input.ProcessPath, input.PathSeperator);
            if (process is null)
                throw new ArgumentException($"A process with path {input.ProcessPath} and path seperator '{input.PathSeperator}' does not exist in HyperThought.");

            // Get the metadata property to update
            HyperthoughtMetadata metadata;
            if (process.Metadata.Exists(item => item.Key == input.PropertyKey))
                if (input.OverwriteExisting == false)
                    throw new ArgumentException($"The property {input.PropertyKey} already exists under {input.ProcessPath} and will not be updated.");
                else
                {
                    metadata = process.Metadata.FirstOrDefault(item => item.Key == input.PropertyKey);
                    metadata.Value = new HyperthoughtMetadataValue
                    {
                        DisplayText = input.FilePath,
                        Link = fileLink,
                        Type = HyperthoughtDataType.Link
                    };
                }
            else
            {
                metadata = new HyperthoughtMetadata
                {
                    Key = input.PropertyKey,
                    Value = new HyperthoughtMetadataValue()
                };
                metadata.Value.DisplayText = input.FilePath;
                metadata.Value.Link = fileLink;
                metadata.Value.Type = HyperthoughtDataType.Link;
                process.Metadata.Add(metadata);
            }

            // Update the Hyperthought process.
            await api.Workflow.UpdateProcessAsync(process);

            return new UpdateHyperthoughtProcessFileLineOperationOut();
        }
    }
}
