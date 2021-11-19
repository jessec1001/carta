using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;


namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Files module of the HyperThought API.
    /// </summary>
    public class HyperthoughtFilesApi
    {
        private HyperthoughtApi Api { get; init; }

        /// <summary>
        /// Gets the files API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new Uri(Api.GetBaseUri(), "files/v1/");


        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtFilesApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api"></param>
        public HyperthoughtFilesApi(HyperthoughtApi api)
        {
            Api = api;
        }

        #region Operations
        /// <summary>
        /// Obtains the HyperThought file for a file under the given UUID directory path and workspace ID.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs</param>
        /// <param name="fileName">The name of the file or folder located under the given directory path</param>
        /// <param name="workspaceId">The workspace ID of the file or folder</param>
        /// <returns>The Hyperthought file object, null if the file object does not exist.</returns>
        public async Task<HyperthoughtFile> GetFileAsync(
            string uuidPath,
            string fileName,
            Guid workspaceId
        )
        {
            Uri requestUri = new Uri(GetApiUri(), $"workspace/{workspaceId}/?path={uuidPath}");
            IList<HyperthoughtFile> hyperthoughtFiles =
                await Api.GetJsonObjectAsync<IList<HyperthoughtFile>>(requestUri);
            HyperthoughtFile hyperthoughtFile =
                hyperthoughtFiles.FirstOrDefault(x => x.Name == fileName);
            return hyperthoughtFile;
        }

        /// <summary>
        /// Creates a new folder under the given UUID directory path.
        /// </summary>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs.</param>
        /// <param name="folderName">The name of the folder located under the given directory path.</param>
        /// <param name="workspaceId">The workspace ID</param>
        /// <returns>A response to the Hyperthought create folder.</returns>
        public async Task<HyperthoughtCreateFolderResponse> CreateFolderAsync(
            string uuidPath,
            string folderName,
            Guid workspaceId
        )
        {
            HyperthoughtCreateFolderRequest createFolder = new HyperthoughtCreateFolderRequest
            {
                WorkspaceId = workspaceId,
                UUIDPath = uuidPath,
                FolderName = folderName
            };
            Uri uri = new Uri(Api.GetBaseUri(), "files/create-folder/");
            HyperthoughtCreateFolderResponse hyperthoughtCreateFolderResponse =
                await Api.PostJsonObjectAsync<HyperthoughtCreateFolderRequest, HyperthoughtCreateFolderResponse>
                (
                    uri,
                    createFolder
                );
            return hyperthoughtCreateFolderResponse;
        }

        /// <summary>
        /// Moves a file with the given UUID to the folder with the given UUID.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <param name="sourceWorkspaceId">The source workspace ID.</param>
        /// <param name="destinationWorkspaceId">The destination workspace ID.</param>
        /// <param name="sourceDirectoryId">The source directory ID.</param>
        /// <param name="destinationDirectoryId">The destination directory ID.</param>
        public async Task MoveFileAsync(
            Guid fileId,
            Guid sourceWorkspaceId,
            Guid destinationWorkspaceId,
            string sourceDirectoryId = null,
            string destinationDirectoryId = null
        )
        {
            // Note: moving files in the root directory requires that the source and destination directory IDs
            // to not be set. Since the Guid do not allow nullable fields, these have been specified as strings.
            HyperthoughtMoveFileRequest hyperthoughtMoveFileRequest = new HyperthoughtMoveFileRequest
            {
                SourceSpaceId = sourceWorkspaceId,
                SourceParentFolderId = sourceDirectoryId,
                DestinationSpaceId = destinationWorkspaceId,
                DestinationParentFolderId = destinationDirectoryId,
                FileIds = new List<Guid> { fileId }
            };
            Uri uri = new Uri(GetApiUri(), "move/");
            await Api.PostJsonObjectAsync<HyperthoughtMoveFileRequest>(uri, hyperthoughtMoveFileRequest);
        }

        /// <summary>
        /// Uploads a file to Hyperthought
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <param name="uuidPath">A HyperThought API directory path: comma-seperated directory UUIDs.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="workspaceId">The workspace ID.</param>
        public async Task UploadFileAsync(
            Stream stream,
            string uuidPath,
            string fileName,
            Guid workspaceId
        )
        {
            HyperthoughtGetUploadUrlRequest hyperthoughtGetUploadUrlRequest = new HyperthoughtGetUploadUrlRequest
            {
                UUIDPath = uuidPath,
                FileName = fileName,
                Size = stream.Length,
                WorkspaceId = workspaceId
            };
            Uri generateUploadUri = new Uri(GetApiUri(), "generate-upload-url/");
            HyperthoughtGetUploadUrlResponse hyperthoughtGetUploadUrlResponse =
                await Api.PostJsonObjectAsync<HyperthoughtGetUploadUrlRequest, HyperthoughtGetUploadUrlResponse>
                (
                    generateUploadUri,
                    hyperthoughtGetUploadUrlRequest
                );

            // Upload the file
            Uri uploadUri = new Uri(new Uri(Api.Access?.BaseUrl), hyperthoughtGetUploadUrlResponse.Uri);
            await Api.PutStreamAsync(uploadUri, stream, fileName);

            // Move the file to permanent storage space
            Uri moveToPermanentUri = new Uri(GetApiUri(), $"{hyperthoughtGetUploadUrlResponse.FileId}/temp-to-perm/");
            await Api.PatchJsonObjectAsync<HyperthoughtTempToPermRequest>(moveToPermanentUri, null);

        }

        /// <summary>
        /// Downloads a file from Hyperthought
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>A stream.</returns>
        public async Task<Stream> DownloadFileAsync(
            Guid fileId
        )
        {
            Uri requestUri = new Uri(new Uri(Api.Access?.BaseUrl), $"api/files/generate-download-url/?id={fileId}");
            HyperthoughtGetDownloadUrlResponse hyperthoughtGetUploadUrlResponse =
                await Api.GetJsonObjectAsync<HyperthoughtGetDownloadUrlResponse>(requestUri);
            return await Api.DownloadFileAsync(new Uri(hyperthoughtGetUploadUrlResponse.Uri));
        }
        #endregion

        #region File Object Link Handling
        /// <summary>
        /// Parses a HyperThoughtfile object link into the file ID.
        /// </summary>
        /// <param name="link">The file object link.</param>
        /// <returns>The parsed file ID if successful; otherwise, returns <see cref="Guid.Empty"/>.</returns>
        public Guid ParseFileObjectLink(string link)
        {
            // Define the regular expression pattern to match against.
            string FILE_ID_GROUP = "fileId";

            Regex pattern = new Regex(@$"^\/files\/filesystementry\/(?<{FILE_ID_GROUP}>[0-9a-f\-]*)$");
            Match match = pattern.Match(link);

            // If a match was found, return the GUID conversion of it.
            // Otherwise, return empty GUID.
            if (match.Success)
            {
                string value = match.Groups[FILE_ID_GROUP].Value;
                if (Guid.TryParse(value, out Guid fileId))
                    return fileId;
                else
                    return Guid.Empty;
            }
            else return Guid.Empty;
        }

        /// <summary>
        /// Computes a HyperThought file object link from the specified file ID.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The file object link to the HyperThought filesystem.</returns>
        public string ComputeFileObjectLink(Guid fileId)
        {
            return $"/files/filesystementry/{fileId}";
        }
        /// <summary>
        /// Computes a HyperThought file object link from the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The file object link to the HyperThought filesystem.</returns>
        public string ComputeFileObjectLink(HyperthoughtFile file)
        {
            return ComputeFileObjectLink(file.PrimaryKey);
        }
        #endregion

        #region File Entry Retrieval
        /// <summary>
        /// Produces a comma-separated and comma-terminated list of directory IDs from an enumerable of IDs.
        /// </summary>
        /// <param name="directoryIds">The enumerable of directory IDs.</param>
        /// <returns>A comma-separated list of directory IDs.</returns>
        protected string ConcatenateDirectories(IEnumerable<Guid> directoryIds)
        {
            string joinedIds = string.Join
            (
                "",
                directoryIds.Select(directoryId => $"{directoryId},")
            );
            return $",{joinedIds}";
        }

        /// <summary>
        /// Obtains a HyperThought file entry from a file ID.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The file entry obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtFile> GetFileAsync(Guid fileId)
        {
            Uri requestUri = new Uri(GetApiUri(), $"?id={fileId}");
            return await Api.GetJsonObjectAsync<HyperthoughtFile>(requestUri);
        }

        /// <summary>
        /// Deletes a HyperThought file entry from a file ID.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>Nothing.</returns>
        public async Task DeleteFileAsync(Guid fileId)
        {
            Uri requestUri = GetApiUri();
            await Api.DeleteAsync<HyperthoughtDeleteFile>(requestUri, new HyperthoughtDeleteFile { Id = fileId });
        }
        /// <summary>
        /// Deletes a HyperThought file entry.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>Nothing.</returns>
        public async Task DeleteFileAsync(HyperthoughtFile file)
        {
            await DeleteFileAsync(file.PrimaryKey);
        }

        /// <summary>
        /// Obtains a list of HyperThought file entries within a specific space specified by parent directories.
        /// </summary>
        /// <param name="space">The type of HyperThought file space.</param>
        /// <param name="spaceId">The unique ID of the HyperThought file space.</param>
        /// <param name="directoryIds">An, in-order, enumerable of parent directories.</param>
        /// <returns>A list of contained file entries obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtFile>> GetDirectoryContentsAsync(
            HyperthoughtFileSpace space,
            Guid? spaceId = null,
            IEnumerable<Guid> directoryIds = null
        )
        {
            if (directoryIds is null)
                directoryIds = Enumerable.Empty<Guid>();

            string methodParameters = "";
            switch (space)
            {
                case HyperthoughtFileSpace.User:
                    methodParameters = "method=user_files";
                    break;
                case HyperthoughtFileSpace.Group:
                    methodParameters = $"method=group_files&group={spaceId}";
                    break;
                case HyperthoughtFileSpace.Project:
                    methodParameters = $"method=project_files&project={spaceId}";
                    break;
            }
            Uri requestUri = new Uri(GetApiUri(), $"?path={ConcatenateDirectories(directoryIds)}&{methodParameters}");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtFile>>(requestUri);
        }

        /// <summary>
        /// Obtains a list of HyperThought file entries within a group space specified by parent directories.
        /// </summary>
        /// <param name="group">The HyperThought group.</param>
        /// <param name="directoryIds">An, in-order, enumerable of parent directories.</param>
        /// <returns>A list of contained file entries obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtFile>> GetDirectoryContentsAsync(
            IEnumerable<Guid> directoryIds,
            HyperthoughtGroup group
        )
        {
            return await GetDirectoryContentsAsync
            (
                HyperthoughtFileSpace.Group,
                group.Content.PrimaryKey,
                directoryIds
            );
        }
        /// <summary>
        /// Obtains a list of HyperThought file entries within a project space specified by parent directories.
        /// </summary>
        /// <param name="project">The HyperThought project.</param>
        /// <param name="directoryIds">An, in-order, enumerable of parent directories.</param>
        /// <returns>A list of contained file entries obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtFile>> GetDirectoryContentsAsync(
            IEnumerable<Guid> directoryIds,
            HyperthoughtProject project
        )
        {
            return await GetDirectoryContentsAsync
            (
                HyperthoughtFileSpace.Project,
                project.Content.PrimaryKey,
                directoryIds
            );
        }
        #endregion

        #region Path Matching
        /// <summary>
        /// Obtains a file entry specified by a delimiter-separated path.
        /// </summary>
        /// <param name="path">
        /// The delimeter-separated path in the form of <c>"Directory1.Director2.File"</c> and so forth.
        /// </param>
        /// <param name="delimiter">The text to specify a delimeter. Defaults to <c>"."</c>.</param>
        /// <param name="space">The type of HyperThought file space.</param>
        /// <param name="spaceId">The unique ID of the HyperThought file space.</param>
        /// <returns>The file specified by a path if it exists. Otherwise, returns <c>null</c>.</returns>
        public async Task<HyperthoughtFile> GetFileFromPathAsync(
            string path,
            string delimiter = ".",
            HyperthoughtFileSpace space = HyperthoughtFileSpace.User,
            Guid? spaceId = null
        )
        {
            // Get the parts of the path.
            if (path is null) return null;
            string[] pathParts = path.Split(delimiter);

            // Recursively search for the file entry satisfying the path.
            HyperthoughtFile retrievedFile = null;
            List<Guid> directoryIds = new List<Guid>();
            for (int k = 0; k < pathParts.Length; k++)
            {
                IList<HyperthoughtFile> files = await GetDirectoryContentsAsync(space, spaceId, directoryIds);
                HyperthoughtFile file = files
                    .Where(file => file.Name == pathParts[k])
                    .FirstOrDefault();
                if (file is null) return null;
                retrievedFile = file;
            }
            return retrievedFile;
        }
        /// <summary>
        /// Obtains the UUID for a file entry specified by a delimiter-separated path.
        /// </summary>
        /// <param name="path">
        /// The delimeter-separated path in the form of <c>"Directory1.Director2.File"</c> and so forth.
        /// </param>
        /// <param name="delimiter">The text to specify a delimeter. Defaults to <c>"."</c>.</param>
        /// <param name="space">The type of HyperThought file space.</param>
        /// <param name="spaceId">The unique ID of the HyperThought file space.</param>
        /// <returns>
        /// The UUID of the file specified by a path if it exists. Otherwise, returns <see cref="Guid.Empty"/>.
        /// </returns>
        public async Task<Guid> GetFileIdFromPathAsync(
            string path,
            string delimiter = ".",
            HyperthoughtFileSpace space = HyperthoughtFileSpace.User,
            Guid? spaceId = null
        )
        {
            HyperthoughtFile file = await GetFileFromPathAsync(path, delimiter, space, spaceId);
            if (file is null) return Guid.Empty;
            else return file.PrimaryKey;
        }
        #endregion

        #region DTOs
        /// <summary>
        /// Represents a data-transfer object for deleting a HyperThought file entry.
        /// </summary>
        private class HyperthoughtDeleteFile
        {
            /// <summary>
            /// The ID of the file.
            /// </summary>
            [JsonPropertyName("id")]
            public Guid Id { get; set; }
        }
        #endregion
    }
}