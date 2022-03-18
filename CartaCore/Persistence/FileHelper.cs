using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Provides helper functions for working with files.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Reads a file asynchronously.
        /// If the file is locked by another process, this method will wait for it to become available.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="lockDelay">The delay between attempts to read the file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A file stream for the file if successful; <c>null</c> otherwise.</returns>
        public static async Task<FileStream> ReadFileAsync(string path, int? lockDelay, CancellationToken token = default)
        {
            // Check that the file actually exists.
            if (!File.Exists(path)) return null;

            // If the file is locked, wait for it to become available.
            while (!token.IsCancellationRequested)
            {
                try { return File.OpenRead(path); }
                catch (IOException ex)
                {
                    if (lockDelay is null) throw ex;
                    await Task.Delay(lockDelay.Value, token);
                }
            }
            return null;
        }
        /// <summary>
        /// Writes a file asynchronously.
        /// If the file is locked by another process, this method will wait for it to become available.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="stream">The stream to write.</param>
        /// <param name="lockDelay">The delay between attempts to write the file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns><c>true</c> if the write was successful; <c>false</c> otherwise.</returns>
        public static async Task<bool> WriteFileAsync(string path, Stream stream, int? lockDelay, CancellationToken token = default)
        {
            // Create all the directories in the path.
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // If the file is locked, wait for it to become available.
            FileStream fileStream = null;
            while (!token.IsCancellationRequested)
            {
                try { fileStream = File.Open(path, FileMode.Create, FileAccess.Write); break; }
                catch (IOException ex)
                {
                    if (lockDelay is null) throw ex;
                    await Task.Delay(lockDelay.Value, token);
                }
            }

            // Write the stream to the file.
            if (fileStream is not null)
            {
                // We copy the stream to the file.
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                fileStream.Dispose();
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Deletes a file asynchronously.
        /// If the file is locked by another process, this method will wait for it to become available.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="lockDelay">The delay between attempts to write the file.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns><c>true</c> if the delete was successful; <c>false</c> otherwise.</returns>
        public static async Task<bool> DeleteFileAsync(string path, int? lockDelay, CancellationToken token = default)
        {
            // Check that the file actually exists.
            if (!File.Exists(path)) return false;

            // If the file is locked, wait for it to become available.
            while (!token.IsCancellationRequested)
            {
                try { File.Delete(path); return true; }
                catch (IOException ex)
                {
                    if (lockDelay is null) throw ex;
                    await Task.Delay(lockDelay.Value, token);
                }
            }
            return false;
        }
    }
}