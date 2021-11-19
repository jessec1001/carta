using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Operations.Hyperthought
{
    /// <summary>
    /// The input for the <see cref="HyperthoughtFileOperation" /> operation.
    /// </summary>
    public struct InputHyperthoughtFileOperation
    {
        /// <summary>
        /// The input file's name, including the file path, e.g. /dirname/subdirname/filename.txt
        /// </summary>
        public string InputFilePath;

        /// <summary>
        /// The alias of the workspace the file lives under
        /// </summary>
        public string WorkspaceAlias;

        /// <summary>
        /// The input file stream
        /// </summary>
        public Stream InputStream;
    }

    /// <summary>
    /// The output for the <see cref="HyperthoughtFileOperation" /> operation.
    /// </summary>
    public struct OutputHyperthoughtFileOperation
    {
        // Note: InputFilePath and WorkspaceAlias are included in the output, so that this operation can
        // easily be tied together with the HyperthoughtProcessUpdateFileLinkOperation

        /// <summary>
        /// The full file name and path
        /// </summary>
        public string InputFilePath;

        /// <summary>
        /// The alias of the workspace the file lives under
        /// </summary>
        public string WorkspaceAlias;

        /// <summary>
        /// The output file stream
        /// </summary>
        public Stream OutputStream;
    }

    /// <summary>
    /// An operation that can upload and/or download a stream to/from HyperThought.
    /// </summary>
    public class HyperthoughtFileOperation
    {
        /// <summary>
        /// The Hyperthought Workspaces API
        /// </summary>
        protected HyperthoughtWorkspacesApi HyperthoughtWorkspacesApi;


        /// <summary>
        /// The Hyperthought Files API
        /// </summary>
        protected HyperthoughtFilesApi HyperthoughtFilesApi;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hyperthoughtApi">The Hyperthought API</param>
        public HyperthoughtFileOperation(HyperthoughtApi hyperthoughtApi)
        {
            HyperthoughtWorkspacesApi = new HyperthoughtWorkspacesApi(hyperthoughtApi);
            HyperthoughtFilesApi = new HyperthoughtFilesApi(hyperthoughtApi);
        }

        /// <summary>
        /// Retrieves the workspace UUID of the workspace with the given alias. Note that Hyperthought requires unique
        /// aliases for workspaces, but allows for duplicate workspace names. For this reason, files are specified
        /// by workspace alias, rather than workspace name.
        /// </summary>
        /// <param name="workspaceAlias">The workspace alias</param>
        protected async Task<Guid> GetWorkspaceId(string workspaceAlias)
        {
            IList<HyperthoughtWorkspace> workspaces = await HyperthoughtWorkspacesApi.GetWorkspacesAsync();
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
                await HyperthoughtFilesApi.GetFileAsync(uuidPath, folderName, workspaceId);
            if (hyperthoughtDir is null) return null;
            else return $"{uuidPath}{hyperthoughtDir.PrimaryKey},";
        }

        /// <summary>
        /// Obtains the HyperThought file UUID for a file under the given UUID directory path and workspace ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs</param>
        /// <param name="fileName">The name of the file located under the given directory path</param>
        /// <param name="workspaceId">The workspace ID of the file</param>
        /// <returns>The file UUID; an empty UUID is returned if the file does not exist.</returns>
        protected async Task<Guid> GetFileIdAsync(
            string uuidPath,
            string fileName,
            Guid workspaceId
        )
        {
            HyperthoughtFile hyperthoughtFile =
                await HyperthoughtFilesApi.GetFileAsync(uuidPath, fileName, workspaceId);
            if (hyperthoughtFile is null) return Guid.Empty;
            else return hyperthoughtFile.PrimaryKey;
        }

        /// <summary>
        /// Obtains the HyperThought file download URL for the given file path and workspace ID.
        /// </summary>
        /// <param name="filePath">The file name, including the file path, e.g. /dirname/subdirname/filename.txt.
        /// </param>
        /// <param name="workspaceId">The workspace ID of the file</param>
        /// <returns>The file download URL.</returns>
        protected async Task<Uri> GetDownloadUrlAsync(
            string filePath,
            Guid workspaceId
        )
        {
            if (filePath is null) return null;

            Console.WriteLine($"Getting the UUID for file {filePath}...");
            // First construct the UUID directory path
            string[] pathParts = filePath.Split(@"/");
            string fileName = pathParts[pathParts.Length - 1];
            string uuidPath = ",";
            for (int k = 1; k < (pathParts.Length - 1); k++)
            {
                Console.WriteLine($"...Getting UUIDPath for folder {pathParts[k]} under UUIDPath {uuidPath}...");
                uuidPath = await GetDirectoryIdPathAsync(uuidPath, pathParts[k], workspaceId);
            }

            // Get and return the file download URL
            Console.WriteLine($"...Getting the download URL for file {fileName} under UUIDPath {uuidPath}...");
            HyperthoughtFile hyperthoughtFile =
                await HyperthoughtFilesApi.GetFileAsync(uuidPath, fileName, workspaceId);
            if (hyperthoughtFile is null) return null;
            else return new Uri(hyperthoughtFile.Resources.DownloadUrl);
        }

        /// <summary>
        /// Generates a UUIDPath for the directory a file will be stored under. If the directory does not exist,
        /// it will be created.
        /// </summary>
        /// <param name="directoryPath">The directory path, e.g. /directory/subdirectory </param>
        /// <param name="workspaceId">The workspace ID.</param>
        protected async Task<string> GenerateDirectoryIdPathAsync(string directoryPath, Guid workspaceId)
        {
            Console.WriteLine($"Generating the UUIDPath for directory {directoryPath}...");
            // Construct the UUID directory path, creating sub-directories if they do not exist
            string[] pathParts = directoryPath.Split(@"/");
            string outUuidPath = "";
            string inUuidPath = ",";
            for (int k = 1; k < (pathParts.Length-1); k++)
            {
                Console.WriteLine($"...Getting path for folder {pathParts[k]} under UUIDPath {inUuidPath}...");
                outUuidPath = await GetDirectoryIdPathAsync(inUuidPath, pathParts[k], workspaceId);
                if (outUuidPath is null)
                {
                    Console.WriteLine($"...Creating folder {pathParts[k]} under UUIDPath {inUuidPath}...");
                    HyperthoughtCreateFolderResponse response = await HyperthoughtFilesApi.CreateFolderAsync
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
        /// Gets the UUID for a file given its name
        /// </summary>
        /// <param name="filePath">The file name, including the file path, e.g. /dirname/subdirname/filename.txt</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        protected async Task<Guid> GetFileIdAysnc(string filePath, Guid workspaceId)
        {
            if (filePath is null) return Guid.Empty;

            Console.WriteLine($"Getting the UUID for file {filePath}...");
            // First construct the UUID directory path
            string[] pathParts = filePath.Split(@"/");
            string fileName = pathParts[pathParts.Length - 1];
            string uuidPath = ",";
            for (int k = 1; k < (pathParts.Length-1); k++)
            {
                Console.WriteLine($"...Getting UUIDPath for folder {pathParts[k]} under UUIDPath {uuidPath}...");
                uuidPath = await GetDirectoryIdPathAsync(uuidPath, pathParts[k], workspaceId);
            }

            // Get and return the file UIID
            Console.WriteLine($"...Getting the UUID for file {fileName} under UUIDPath {uuidPath}...");
            Guid fileId = await GetFileIdAsync(uuidPath, fileName, workspaceId);
            Console.WriteLine($"UUID for file {fileName} is {fileId}");
            return fileId;
        }

        /// <summary>
        /// Uploads a stream to HyperThought
        /// </summary>
        /// <param name="inputFilePath">The file name under which the stream will be saved,
        /// including the file path, e.g. /dirname/subdirname/filename.txt</param>
        /// <param name="inputStream">The input stream</param>
        /// <param name="workspaceId">The workspace ID</param>
        protected async Task UploadFileAsync(
            string inputFilePath,
            Stream inputStream,
            Guid workspaceId)
        {
            Console.WriteLine($"Uploading file {inputFilePath} to workspace {workspaceId}...");
            // Get the UUID path to upload file to
            string[] pathParts = inputFilePath.Split(@"/");
            string fileName = pathParts[pathParts.Length - 1];
            string uuidPath = ",";
            if (pathParts.Length > 1)
            {
                string dirPath = @"";
                for (int k = 0; k < (pathParts.Length - 1); k++)
                    dirPath = dirPath + pathParts[k] + @"/";
                uuidPath = await GenerateDirectoryIdPathAsync(dirPath, workspaceId);
            }

            // If the file already exists, first archive it to a new folder
            Guid fileId = await GetFileIdAsync(uuidPath, fileName, workspaceId);
            if (fileId != Guid.Empty)
            {
                string archiveFolderName = "carta_archive_" + DateTime.Now.ToString();
                Console.WriteLine($"...Creating archive folder {archiveFolderName}...");
                HyperthoughtCreateFolderResponse response = await HyperthoughtFilesApi.CreateFolderAsync
                    (
                        uuidPath,
                        archiveFolderName,
                        workspaceId
                    );
                string[] uuidParts = uuidPath.Split(",");
                string sourceDirId = null;
                if (uuidParts.Length > 2)
                    sourceDirId = uuidParts[uuidParts.Length - 2];
                string destinationDirId = response.Document.Content.PrimaryKey.ToString();
                Console.WriteLine($"...Moving file {fileId} in folder with ID {sourceDirId} to folder with ID " +
                    $"{destinationDirId}...");
                await HyperthoughtFilesApi.MoveFileAsync(
                    fileId,
                    workspaceId,
                    workspaceId,
                    sourceDirId,
                    destinationDirId);
            }

            // Upload the file
            Console.WriteLine($"...Uploading {fileName} to UUIDPath {uuidPath}...");
            await HyperthoughtFilesApi.UploadFileAsync
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
        /// <param name="inputFilePath">The file name to download, including the file path,
        /// e.g. /dirname/subdirname/filename.txt</param>
        /// <param name="workspaceId">The workspace ID of the file.</param>
        /// <returns>An output stream.</returns>
        protected async Task<Stream> DownloadFileAsync(string inputFilePath, Guid workspaceId)
        {
            Console.WriteLine($"Downloading {inputFilePath}...");
            Guid fileId = await GetFileIdAysnc(inputFilePath, workspaceId);
            if (fileId == Guid.Empty)
            {
                throw new ArgumentException($"File {inputFilePath} does not exist", "InputFilePath");
            }
            else
            {
                Console.WriteLine($"...Downloading with UUID {fileId}...");
                Stream outputStream = await HyperthoughtFilesApi.DownloadFileAsync(fileId);
                return outputStream;
            }
        }

        /// <summary>
        /// Perform the upload/download file operation.
        /// </summary>
        /// <param name="inputHyperthoughtFileOperation">An <see cref="InputHyperthoughtFileOperation" /> instance.
        /// </param>
        /// <returns>An <see cref="OutputHyperthoughtFileOperation" /> instance</returns>
        public async Task<OutputHyperthoughtFileOperation> Perform(
            InputHyperthoughtFileOperation inputHyperthoughtFileOperation)
        {
            // Validate inputs
            Guid workspaceId = await GetWorkspaceId(inputHyperthoughtFileOperation.WorkspaceAlias);
            if (workspaceId.Equals(Guid.Empty))
                throw new ArgumentException($"Workspace alias {inputHyperthoughtFileOperation.WorkspaceAlias} " +
                    $"does not exist", "WorkspaceAlias");
            if (inputHyperthoughtFileOperation.InputFilePath is null)
                throw new ArgumentException($"InputFilePath must be specified", "InputFilePath");
            if (inputHyperthoughtFileOperation.InputFilePath.Split(@"/").Length > 1)
                if (!inputHyperthoughtFileOperation.InputFilePath.StartsWith(@"/"))
                    throw new ArgumentException($"InputFilePath does not start with /", "InputFilePath");

            // Perform operations
            if (inputHyperthoughtFileOperation.InputStream is null)
            {
                Stream outputStream = await DownloadFileAsync
                (
                    inputHyperthoughtFileOperation.InputFilePath,
                    workspaceId
                );
                return new OutputHyperthoughtFileOperation
                {
                    InputFilePath = inputHyperthoughtFileOperation.InputFilePath,
                    WorkspaceAlias = inputHyperthoughtFileOperation.WorkspaceAlias,
                    OutputStream = outputStream
                };
            }
            else
            {
                await UploadFileAsync
                (
                    inputHyperthoughtFileOperation.InputFilePath,
                    inputHyperthoughtFileOperation.InputStream,
                    workspaceId
                );
                inputHyperthoughtFileOperation.InputStream.Seek(0, SeekOrigin.Begin);
                return new OutputHyperthoughtFileOperation
                {
                    InputFilePath = inputHyperthoughtFileOperation.InputFilePath,
                    WorkspaceAlias = inputHyperthoughtFileOperation.WorkspaceAlias,
                    OutputStream = inputHyperthoughtFileOperation.InputStream
                };
            }
        }
    }
}
