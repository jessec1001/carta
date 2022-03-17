using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CartaWeb.Models.DocumentItem;
using CartaCore.Operations;

namespace CartaWeb.Services
{
    /// <summary>
    /// A queue that provides a mechanism for storing jobs to be run in the background.
    /// </summary>
    public class BackgroundJobQueue
    {
        /// <summary>
        /// Stores the jobs awaiting execution.
        /// </summary>
        private ConcurrentQueue<OperationJob> Jobs = new();
        /// <summary>
        /// Ensures that multiple threads can await access the queue safely.
        /// </summary>
        private readonly SemaphoreSlim Signal = new SemaphoreSlim(0);

        /// <summary>
        /// Seeks a job from the queue with a specified identifier.
        /// </summary>
        /// <param name="jobId">The identifier of the job.</param>
        /// <returns>The matching <see cref="OperationJob" /> if found; otherwise, <c>null</c>.</returns>
        public OperationJob Seek(string jobId)
        {
            foreach (OperationJob job in Jobs)
                if (job.Id == jobId) return job; 
            return null;
        }
        /// <summary>
        /// Pushes a job to the queue.
        /// </summary>
        /// <param name="job">The job.</param>
        public void Push(OperationJob job)
        {
            Jobs.Enqueue(job);
            Signal.Release();
        }
        /// <summary>
        /// Pops a job from the queue.
        /// If no items are in the queue, this method will block until an item is pushed.
        /// </summary>
        /// <returns>The job.</returns>
        public async Task<OperationJob> Pop(CancellationToken cancellationToken)
        {
            await Signal.WaitAsync(cancellationToken);

            Jobs.TryDequeue(out OperationJob job);
            return job;
        }
    }
}