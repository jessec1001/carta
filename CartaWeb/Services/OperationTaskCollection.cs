using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CartaWeb.Models.DocumentItem;
using CartaCore.Operations;

namespace CartaWeb.Services
{
    public class OperationJobCollection
    {
        // TODO: The structure of the items in this queue look like it good candidates for a more efficient data structure (job).
        // TODO: We can try to use an events system to allow jobs to batch database requests.
        private ConcurrentQueue<(JobItem, Operation, OperationContext)> Tasks = new();
        private readonly SemaphoreSlim Signal = new SemaphoreSlim(0);

        public (JobItem, Operation, OperationContext) Seek(string jobId)
        {
            foreach ((JobItem job, Operation operation, OperationContext context) in Tasks)
            {
                if (job.Id == jobId)
                    return (job, operation, context);
            }
            return (null, null, null);
        }
        public void Push((JobItem jobItem, Operation operation, OperationContext context) item)
        {
            Tasks.Enqueue(item);
            Signal.Release();
        }
        public async Task<(JobItem jobItem, Operation operation, OperationContext context)> Pop()
        {
            await Signal.WaitAsync();

            Tasks.TryDequeue(out var item);
            return item;
        }
    }
}