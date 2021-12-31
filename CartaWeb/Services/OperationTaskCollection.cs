using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CartaWeb.Models.DocumentItem;
using CartaCore.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace CartaWeb.Services
{
    public class OperationTaskCollection
    {
        private ConcurrentQueue<Func<IServiceScopeFactory, (JobItem, Operation, OperationContext)>> Tasks = new();
        private readonly SemaphoreSlim Signal = new SemaphoreSlim(0);

        public void Push(Func<IServiceScopeFactory, (JobItem jobItem, Operation job, OperationContext context)> item)
        {
            Tasks.Enqueue(item);
            Signal.Release();
        }
        public async Task<Func<IServiceScopeFactory, (JobItem jobItem, Operation operation, OperationContext context)>> Pop()
        {
            await Signal.WaitAsync();

            Tasks.TryDequeue(out var item);
            return item;
        }
    }
}