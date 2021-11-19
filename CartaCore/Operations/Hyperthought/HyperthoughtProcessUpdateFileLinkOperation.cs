using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Operations.Hyperthought
{
    /// <summary>
    /// The input for the <see cref="HyperthoughtProcesssUpdateFileLinkOperation" /> operation.
    /// </summary>
    public struct InputHyperthoughtProcessUpdateFileLinkOperation
    {
        /// <summary>
        /// The input file's name, including the file path, e.g. /dirname/subdirname/filename.txt
        /// </summary>
        public string InputFilePath;

        /// <summary>
        /// The alias of the workspace the file is stored under. 
        /// </summary>
        public string WorkspaceAlias;

        /// <summary>
        /// The key (name) of the property the file should be linked to. 
        /// </summary>
        public string PropertyKey;

        /// <summary>
        /// The full path of the process, e.g. /source/resource/workflow/builds/part/a1/results.
        /// </summary>
        public string ProcessPath;

        /// <summary>
        /// The process path seperator. Defaults to '.'
        /// </summary>
        public string PathSeperator;

        /// <summary>
        /// Overwrite the value if the property with the given key already exists. Defaults to true. If set to false,
        /// and the property already exists, the operation will exit with a warning message.
        /// </summary>
        public bool? OverwriteExisting;
    }

    /// <summary>
    /// An operation that can upload and/or download a stream to/from HyperThought.
    /// </summary>
    public class HyperthoughtProcesssUpdateFileLinkOperation
    {
        /// <summary>
        /// The Hyperthought Workspaces API
        /// </summary>
        protected HyperthoughtWorkspacesApi WorkspacesApi;

        /// <summary>
        /// The Hyperthought Files API
        /// </summary>
        protected HyperthoughtFilesApi FilesApi;

        /// <summary>
        /// The Hyperthought Workflow API
        /// </summary>
        protected HyperthoughtWorkflowApi WorkflowApi;

        /// <summary>
        /// Creates a new instance of the <see cref="HyperthoughtProcesssUpdateFileLinkOperation"/> class 
        /// </summary>
        /// <param name="api">An instance of the <see cref="HyperthoughtApi"/> class, with the necessary
        /// Hyperthought API credentials</param>
        public HyperthoughtProcesssUpdateFileLinkOperation(HyperthoughtApi api)
        {
            WorkspacesApi = new HyperthoughtWorkspacesApi(api);
            FilesApi = new HyperthoughtFilesApi(api);
            WorkflowApi = new HyperthoughtWorkflowApi(api);
        }

        /// <summary>
        /// Retrieves the workspace UUID of the workspace with the given alias. Note that Hyperthought requires unique
        /// aliases for workspaces, but allows for duplicate workspace names. For this reason, files are specified
        /// by workspace alias, rather than workspace name.
        /// </summary>
        /// <param name="workspaceAlias">The workspace alias</param>
        protected async Task<Guid> GetWorkspaceId(string workspaceAlias)
        {
            IList<HyperthoughtWorkspace> workspaces = await WorkspacesApi.GetWorkspacesAsync();
            HyperthoughtWorkspace workspace = workspaces.FirstOrDefault(item => item.Alias == workspaceAlias);
            if (workspace is null) return Guid.Empty;
            else return workspace.PrimaryKey;
        }

        /// <summary>
        /// Obtains the HyperThought UUID directory path for a folder under the given UUID directory path and workspace
        /// ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs</param>
        /// <param name="folderName">The name of the folder located under the given directory path</param>
        /// <param name="workspaceId">The workspace ID of the folder</param>
        /// <returns>A string of UUIDs representing a HyperThought API directory path.</returns>
        protected async Task<string> GetDirectoryIdPathAsync(
            string uuidPath,
            string folderName,
            Guid workspaceId
        )
        {
            HyperthoughtFile hyperthoughtDir =
                await FilesApi.GetFileAsync(uuidPath, folderName, workspaceId);
            if (hyperthoughtDir is null) return null;
            else return $"{uuidPath}{hyperthoughtDir.PrimaryKey},";
        }

        /// <summary>
        /// Obtains the HyperThought file link for a file under the given UUID directory path and workspace ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs</param>
        /// <param name="fileName">The name of the file located under the given directory path</param>
        /// <param name="workspaceId">The workspace ID of the file</param>
        /// <returns>The file link; null is returned if the file does not exist.</returns>
        protected async Task<string> GetFileLinkAsync(
            string uuidPath,
            string fileName,
            Guid workspaceId
        )
        {
            if (uuidPath is null) return null;
            HyperthoughtFile hyperthoughtFile = await FilesApi.GetFileAsync(uuidPath, fileName, workspaceId);
            if (hyperthoughtFile is null)
                return null;
            else
                // Note that the Hyperthought API does not have the file link returned in any of its response objects,
                // and versioning of files are not yet implemented.
                // From inspection of metadata stored under a Hyperthought Process node, the file link is set to the
                // file canonical URI, with "/versions/0.json" appended to it. This may change in future. 
                return $"{hyperthoughtFile.CanonicalUri}/versions/0.json";
        }

        /// <summary>
        /// Gets the Hyperthought file link for a file given its name
        /// </summary>
        /// <param name="filePath">The file name, including the file path, e.g. /dirname/subdirname/filename.txt</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <returns>The file link; null is returned if the file does not exist.</returns>
        protected async Task<string> GetFileLinkAysnc(string filePath, Guid workspaceId)
        {
            Console.WriteLine($"Getting the UUID for file {filePath}...");
            // First construct the UUID directory path
            string[] pathParts = filePath.Split(@"/");
            string fileName = pathParts[pathParts.Length - 1];
            string uuidPath = ",";
            for (int k = 1; k < (pathParts.Length - 1); k++)
            {
                Console.WriteLine($"...Getting UUIDPath for folder {pathParts[k]} under workspace UUID " +
                    $"{workspaceId} and UUIDPath {uuidPath}...");
                uuidPath = await GetDirectoryIdPathAsync(uuidPath, pathParts[k], workspaceId);
            }

            // Get and return the file link
            Console.WriteLine($"...Getting the link for file {fileName} under workspace UUID " +
                $"{workspaceId} and UUDIPath {uuidPath}...");
            string fileLink = await GetFileLinkAsync(uuidPath, fileName, workspaceId);
            Console.WriteLine($"File link for file {fileName} is {fileLink}");
            return fileLink;
        }
        
        /// <summary>
        /// Update the process node with the file link
        /// </summary>
        /// <param name="input">An <see cref="InputHyperthoughtProcessUpdateFileLinkOperation" /> instance.
        /// </param>
        public async Task Perform(
            InputHyperthoughtProcessUpdateFileLinkOperation input)
        {
            // Set defaults
            if (input.PathSeperator is null) input.PathSeperator = ".";
            if (!input.OverwriteExisting.HasValue) input.OverwriteExisting = true;

            // Validate inputs
            Guid workspaceId = await GetWorkspaceId(input.WorkspaceAlias);
            if (workspaceId.Equals(Guid.Empty))
                throw new ArgumentException($"Workspace alias {input.WorkspaceAlias} " +
                    $"does not exist", "WorkspaceAlias");
            if (input.InputFilePath is null)
                throw new ArgumentException($"InputFilePath must be specified", "InputFilePath");
            if (input.InputFilePath.Split(@"/").Length > 1)
                if (!input.InputFilePath.StartsWith(@"/"))
                    throw new ArgumentException($"InputFilePath does not start with /", "InputFilePath");

            // Get the file link
            string fileLink = await GetFileLinkAysnc(input.InputFilePath, workspaceId);
            if (fileLink is null)
                throw new ArgumentException($"The file {input.InputFilePath} does not exist in HyperThought",
                    input.ProcessPath);

            // Hyperthought does NOT want the path to start with a seperator - strip that off if need be
            if (input.ProcessPath.StartsWith(input.PathSeperator))
                input.ProcessPath = input.ProcessPath.Substring(1);

            // Get the process object
            HyperthoughtProcess
                process = await WorkflowApi.GetProcessFromPathAsync(input.ProcessPath, input.PathSeperator);
            if (process is null)
                throw new ArgumentException($"A process with path {input.ProcessPath} and path seperator " +
                    $"'{input.PathSeperator}' does not exist in HyperThought", input.ProcessPath);

            // Get the metadata property to update
            HyperthoughtMetadata metadata;
            if (process.Metadata.Exists(item => item.Key == input.PropertyKey))
                if (input.OverwriteExisting == false)
                    throw new ArgumentException($"The property {input.PropertyKey} already exists under " +
                        $"{input.ProcessPath} and will NOT be updated");
                else
                {
                    metadata = process.Metadata.FirstOrDefault(item => item.Key == input.PropertyKey);
                    metadata.Value = new HyperthoughtMetadataValue();
                    metadata.Value.DisplayText = input.InputFilePath;
                    metadata.Value.Link = fileLink;
                    metadata.Value.Type = HyperthoughtDataType.Link;
                }
            else
            {
                metadata = new HyperthoughtMetadata();
                metadata.Key = input.PropertyKey;
                metadata.Value = new HyperthoughtMetadataValue();
                metadata.Value.DisplayText = input.InputFilePath;
                metadata.Value.Link = fileLink;
                metadata.Value.Type = HyperthoughtDataType.Link;
                process.Metadata.Add(metadata);
            }
                
            // Update the Hyperthought process
            await WorkflowApi.UpdateProcessAsync(process);
            
        }
    }
}
