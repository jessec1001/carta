using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Represents a context for accessing the filesystem.
    /// </summary>
    public class FileDbContext : INoSqlDbContext
    {
        /// <summary>
        /// The path to where files should be stored.
        /// </summary>
        public string SystemPath { get; private init; }
        /// <summary>
        /// The delay to use when a file is locked by some other process.
        /// </summary>
        public int? LockDelay { get; private init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDbContext"/> class.
        /// </summary>
        /// <param name="path">The path under which files should be stored.</param>
        /// <param name="lockDelay">The delay between checks to a locked file.</param>
        public FileDbContext(string path, int? lockDelay)
        {
            SystemPath = path;
            LockDelay = lockDelay;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDbContext"/> class.
        /// </summary>
        /// <param name="path">The path under which files should be stored.</param>
        public FileDbContext(string path) : this(path, null) { }

        private async Task<FileStream> ReadFileAsync(string path, CancellationToken token = default)
        {
            // Check that the file actually exists.
            if (!File.Exists(path)) return null;

            // If the file is locked, wait for it to become available.
            while (!token.IsCancellationRequested)
            {
                try { return File.OpenRead(path); }
                catch (IOException ex)
                {
                    if (LockDelay is null) throw ex;
                    await Task.Delay(LockDelay.Value, token);
                }
            }
            return null;
        }
        private async Task<bool> WriteFileAsync(string path, string json, CancellationToken token = default)
        {
            // Create all the directories in the path.
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // If the file is locked, wait for it to become available.
            FileStream fileStream = null;
            while (!token.IsCancellationRequested)
            {
                try { fileStream = File.OpenWrite(path); }
                catch (IOException ex)
                {
                    if (LockDelay is null) throw ex;
                    await Task.Delay(LockDelay.Value, token);
                }
            }

            // Write the stream to the file.
            if (fileStream is not null)
            {
                using (StreamWriter streamWriter = new(fileStream))
                    await streamWriter.WriteAsync(json);
                return true;
            }
            else return false;
        }
        private async Task<bool> DeleteFileAsync(string path, CancellationToken token = default)
        {
            // Check that the file actually exists.
            if (!File.Exists(path)) return false;

            // If the file is locked, wait for it to become available.
            while (!token.IsCancellationRequested)
            {
                try { File.Delete(path); return true; }
                catch (IOException ex)
                {
                    if (LockDelay is null) throw ex;
                    await Task.Delay(LockDelay.Value, token);
                }
            }
            return false;
        }

        /// <inheritdoc />
        public async Task<DbDocument> ReadDocumentAsync(string partitionKey, string sortKey)
        {
            // Get the path to the document.
            string path = Path.Join(SystemPath, partitionKey, $"{sortKey}.json");
            
            // Read the file and generate the document.
            FileStream stream = await ReadFileAsync(path);
            if (stream is null) return null;
            else
            {
                string jsonString = await new StreamReader(stream).ReadToEndAsync();
                return new DbDocument(partitionKey, sortKey, jsonString, DbOperationType.Read);
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<DbDocument> ReadDocumentsAsync(string partitionKey, string sortKeyPrefix)
        {
            // Get the path to the documents directory.
            string path = Path.Join(SystemPath, partitionKey);

            // Enumerate over each item in the documents directory.
            foreach (string file in Directory.EnumerateFiles(path, $"{sortKeyPrefix}*.json"))
            {
                // Read the file and generate the document.
                FileStream stream = await ReadFileAsync(file);
                if (stream is null) continue;
                else
                {
                    string sortKey = Path.GetFileNameWithoutExtension(file);
                    string jsonString = new StreamReader(stream).ReadToEnd();
                    yield return new DbDocument(partitionKey, sortKey, jsonString, DbOperationType.Read);
                }
            }
        }
        /// <inheritdoc />
        public async Task<bool> WriteDocumentAsync(DbDocument dbDocument)
        {
            // Get the path to the document.
            string path = Path.Join(SystemPath, dbDocument.PartitionKey, $"{dbDocument.SortKey}.json");

            // Handle the different database operations.
            switch (dbDocument.Operation)
            {
                // We should not be reading a document within this method.
                case DbOperationType.Read:
                    return false;
                
                // Handle document deletion.
                case DbOperationType.Delete:
                    if (File.Exists(path))
                        return await DeleteFileAsync(path);
                    else
                        return false;

                // Handle document creation.
                case DbOperationType.Create:
                    if (File.Exists(path))
                        return false;
                    else
                        return await WriteFileAsync(path, dbDocument.JsonString);

                // Handle document update.
                case DbOperationType.Update:
                    return await WriteFileAsync(path, dbDocument.JsonString);
            }
            return false;
        }
        /// <inheritdoc />
        public async Task<bool> WriteDocumentsAsync(IEnumerable<DbDocument> dbDocuments)
        {
            bool success = true;
            foreach (DbDocument dbDocument in dbDocuments)
                success &= await WriteDocumentAsync(dbDocument);
            return success;
        }
    }
}