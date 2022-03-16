using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Operations.Attributes;
using CartaCore.Operations.Authentication;

namespace CartaCore.Operations.Hyperthought
{
    /// <summary>
    /// The input for the <see cref="HyperthoughtFileOperation" /> operation.
    /// </summary>
    public struct HyperthoughtFileOperationIn
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
        /// The alias of the workspace the file lives under.
        /// </summary>
        [FieldRequired]
        [FieldName("Workspace Alias")]
        public string WorkspaceAlias { get; set; }

        /// <summary>
        /// The stream of data to be uploaded to HyperThought. If not specified, no data is uploaded.
        /// </summary>
        [FieldName("Stream")]
        public Stream Stream { get; set; }
    }

    /// <summary>
    /// The output for the <see cref="HyperthoughtFileOperation" /> operation.
    /// </summary>
    public struct HyperthoughtFileOperationOut
    {
        // TODO: For this type of operation, we need a way of specifying a connection that creates a dependency without
        //       necessarily needing to pass data. For this, we can specify null for the connection target (or source)
        //       field.

        /// <summary>
        /// The stream of data downloaded from HyperThought.
        /// </summary>
        [FieldName("Stream")]
        public Stream OutputStream { get; set; }
    }

    /// <summary>
    /// Uploads and/or downloads a steams to/from HyperThought as a file.
    /// </summary>
    [OperationName(Display = "HyperThought File", Type = "hyperthoughtFile")]
    [OperationTag(OperationTags.Hyperthought)]
    [OperationTag(OperationTags.Saving)]
    [OperationTag(OperationTags.Loading)]
    public class HyperthoughtFileOperation : TypedOperation
    <
        HyperthoughtFileOperationIn,
        HyperthoughtFileOperationOut
    >
    {
        /// <summary>
        /// Retrieves the workspace UUID of the workspace with the given alias. Note that Hyperthought requires unique
        /// aliases for workspaces, but allows for duplicate workspace names. For this reason, files are specified
        /// by workspace alias, rather than workspace name.
        /// </summary>
        /// <param name="workspaceAlias">The workspace alias.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
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
        /// <param name="api">The authenticated HyperThought API.</param>
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
        /// Obtains the HyperThought file UUID for a file under the given UUID directory path and workspace ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs</param>
        /// <param name="fileName">The name of the file located under the given directory path</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
        /// <returns>The file UUID; an empty UUID is returned if the file does not exist.</returns>
        private static async Task<Guid> GetFileIdAsync(
            string uuidPath,
            string fileName,
            Guid workspaceId,
            HyperthoughtApi api
        )
        {
            HyperthoughtFile hyperthoughtFile =
                await api.Files.GetFileAsync(uuidPath, fileName, workspaceId);
            if (hyperthoughtFile is null) return Guid.Empty;
            else return hyperthoughtFile.PrimaryKey;
        }

        /// <summary>
        /// Generates a UUIDPath for the directory a file will be stored under. If the directory does not exist,
        /// it will be created.
        /// </summary>
        /// <param name="directoryPath">The directory path, e.g. /directory/subdirectory.</param>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
        /// <returns>A string of UUIDs representing a HyperThought API directory path.</returns>
        private static async Task<string> GenerateDirectoryIdPathAsync(string directoryPath, Guid workspaceId, HyperthoughtApi api)
        {
            // TODO: Add support for logging within operations.
            Console.WriteLine($"Generating the UUIDPath for directory {directoryPath}...");
            // Construct the UUID directory path, creating sub-directories if they do not exist
            string[] pathParts = directoryPath.Split(@"/");
            string outUuidPath = "";
            string inUuidPath = ",";
            for (int k = 1; k < (pathParts.Length - 1); k++)
            {
                Console.WriteLine($"...Getting path for folder {pathParts[k]} under UUIDPath {inUuidPath}...");
                outUuidPath = await GetDirectoryIdPathAsync(inUuidPath, pathParts[k], workspaceId, api);
                if (outUuidPath is null)
                {
                    Console.WriteLine($"...Creating folder {pathParts[k]} under UUIDPath {inUuidPath}...");
                    HyperthoughtCreateFolderResponse response = await api.Files.CreateFolderAsync
                    (
                        inUuidPath,
                        pathParts[k],
                        workspaceId
                    );
                    outUuidPath = $"{inUuidPath}{response.Document.Content.PrimaryKey},";
                }
                inUuidPath = outUuidPath;
            }
            Console.WriteLine($"UUIDPath for directory {directoryPath} is {outUuidPath}");
            return outUuidPath;
        }


        /// <summary>
        /// Gets the UUID for a file given its name.
        /// </summary>
        /// <param name="filePath">The file name, including the file path, e.g. /dirname/subdirname/filename.txt.</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
        /// <returns>The UUID of the file; an empty UUID is returned if the file does not exist.</returns>
        private static async Task<Guid> GetFileIdAysnc(string filePath, Guid workspaceId, HyperthoughtApi api)
        {
            if (filePath is null) return Guid.Empty;

            Console.WriteLine($"Getting the UUID for file {filePath}...");
            // First construct the UUID directory path
            string[] pathParts = filePath.Split(@"/");
            string fileName = pathParts[^1];
            string uuidPath = ",";
            for (int k = 1; k < (pathParts.Length - 1); k++)
            {
                Console.WriteLine($"...Getting UUIDPath for folder {pathParts[k]} under UUIDPath {uuidPath}...");
                uuidPath = await GetDirectoryIdPathAsync(uuidPath, pathParts[k], workspaceId, api);
            }

            // Get and return the file UIID
            Console.WriteLine($"...Getting the UUID for file {fileName} under UUIDPath {uuidPath}...");
            Guid fileId = await GetFileIdAsync(uuidPath, fileName, workspaceId, api);
            Console.WriteLine($"UUID for file {fileName} is {fileId}");
            return fileId;
        }

        /// <summary>
        /// Uploads a stream to HyperThought.
        /// </summary>
        /// <param name="inputFilePath">
        /// The file name under which the stream will be saved, including the file path,
        /// e.g. /dirname/subdirname/filename.txt.
        /// </param>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
        private static async Task UploadFileAsync(
            string inputFilePath,
            Stream inputStream,
            Guid workspaceId,
            HyperthoughtApi api
        )
        {
            Console.WriteLine($"Uploading file {inputFilePath} to workspace {workspaceId}...");
            // Get the UUID path to upload file to
            string[] pathParts = inputFilePath.Split(@"/");
            string fileName = pathParts[^1];
            string uuidPath = ",";
            if (pathParts.Length > 1)
            {
                string dirPath = @"";
                for (int k = 0; k < (pathParts.Length - 1); k++)
                    dirPath = dirPath + pathParts[k] + @"/";
                uuidPath = await GenerateDirectoryIdPathAsync(dirPath, workspaceId, api);
            }

            // If the file already exists, first archive it to a new folder
            Guid fileId = await GetFileIdAsync(uuidPath, fileName, workspaceId, api);
            if (fileId != Guid.Empty)
            {
                string archiveFolderName = "carta_archive_" + DateTime.Now.ToString();
                Console.WriteLine($"...Creating archive folder {archiveFolderName}...");
                HyperthoughtCreateFolderResponse response = await api.Files.CreateFolderAsync
                (
                    uuidPath,
                    archiveFolderName,
                    workspaceId
                );
                string[] uuidParts = uuidPath.Split(",");
                string sourceDirId = null;
                if (uuidParts.Length > 2)
                    sourceDirId = uuidParts[^2];
                string destinationDirId = response.Document.Content.PrimaryKey.ToString();
                Console.WriteLine($"...Moving file {fileId} in folder with ID {sourceDirId} to folder with ID " +
                    $"{destinationDirId}...");
                await api.Files.MoveFileAsync(
                    fileId,
                    workspaceId,
                    workspaceId,
                    sourceDirId,
                    destinationDirId);
            }

            // Upload the file
            Console.WriteLine($"...Uploading {fileName} to UUIDPath {uuidPath}...");
            await api.Files.UploadFileAsync
            (
                inputStream,
                uuidPath,
                fileName,
                workspaceId
            );
            Console.WriteLine("Uploaded file");
        }

        /// <summary>
        /// Downloads a file stored in Hyperthought to a stream.
        /// </summary>
        /// <param name="inputFilePath">
        /// The file name to download, including the file path,
        /// e.g. /dirname/subdirname/filename.txt
        /// </param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <param name="api">The authenticated HyperThought API.</param>
        /// <returns>An output stream.</returns>
        private static async Task<Stream> DownloadFileAsync(string inputFilePath, Guid workspaceId, HyperthoughtApi api)
        {
            Console.WriteLine($"Downloading {inputFilePath}...");
            Guid fileId = await GetFileIdAysnc(inputFilePath, workspaceId, api);
            if (fileId == Guid.Empty)
            {
                throw new ArgumentException($"File {inputFilePath} does not exist.");
            }
            else
            {
                Console.WriteLine($"...Downloading with UUID {fileId}...");
                Stream outputStream = await api.Files.DownloadFileAsync(fileId);
                return outputStream;
            }
        }

        /// <inheritdoc />
        public override async Task<HyperthoughtFileOperationOut> Perform(HyperthoughtFileOperationIn input)
        {
            // Create an instance of the Hyperthought API integration.
            HyperthoughtApi api = new(input.HyperthoughtAuth.ApiKey);

            // Validate input values.
            Guid workspaceId = await GetWorkspaceId(input.WorkspaceAlias, api);
            if (workspaceId.Equals(Guid.Empty))
                throw new ArgumentException($"Workspace alias {input.WorkspaceAlias} does not exist.");
            if (input.FilePath is null)
                throw new ArgumentException($"File path must be specified.");
            if (input.FilePath.Split(@"/").Length > 1)
            {
                if (!input.FilePath.StartsWith(@"/"))
                    throw new ArgumentException($"File path does not start with '/'.");
            }

            // If the stream was specified as an input, upload the file.
            if (input.Stream is not null)
            {
                await UploadFileAsync
                (
                    input.FilePath,
                    input.Stream,
                    workspaceId,
                    api
                );
            }

            // Download an existing file whether it was just uploaded or not.
            Stream outputStream = await DownloadFileAsync
            (
                input.FilePath,
                workspaceId,
                api
            );
            return new HyperthoughtFileOperationOut { OutputStream = outputStream };
        }
    }
}
