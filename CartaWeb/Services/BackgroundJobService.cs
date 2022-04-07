using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using CartaWeb.Controllers;
using CartaWeb.Models.DocumentItem;
using CartaCore.Extensions.Typing;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Operations;
using CartaCore.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CartaWeb.Services
{
    // TODO: Implement something, perhaps in the task running service, that will automatically handle loading uploaded
    //       files into streams and handle saving streams to downloadable files.

    /// <summary>
    /// A service that is responsible for running operation jobs in the background across multiple threads.
    /// </summary>
    public class BackgroundJobService : BackgroundService
    {
        private readonly BackgroundJobQueue _jobQueue;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly Persistence _persistence;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobService"/> service.
        /// </summary>
        public BackgroundJobService(
            BackgroundJobQueue taskCollection,
            ILogger<BackgroundJobService> logger,
            INoSqlDbContext noSqlDbContext,
            IServiceScopeFactory serviceScopeFactory)
        {
            _jobQueue = taskCollection;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _persistence = new Persistence(noSqlDbContext);
        }

        /// <summary>
        /// Stores a graph under a particular job field.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="job">The job.</param>
        /// <param name="field">The field.</param>
        /// <typeparam name="TVertex">The type of vertex.</typeparam>
        /// <typeparam name="TEdge">The type of edge.</typeparam>
        private static async void StoreGraph<TVertex, TEdge>(
            IEnumerableComponent<TVertex, TEdge> graph,
            OperationJob job,
            string field)
            where TVertex : IVertex<TEdge>
            where TEdge : IEdge
        {
            string directory = Path.Join("jobs", job.Operation.Id, job.Id, field);
            FileGraph<TVertex, TEdge> fileGraph = new(directory, field) { LockDelay = 25 };
            if (graph is IRootedComponent rooted)
                await fileGraph.SetRoots(rooted.Roots());
            await foreach (TVertex vertex in graph.GetVertices())
                await fileGraph.AddVertex(vertex);
        }

        /// <summary>
        /// Stores an updateable object under a particular job field.
        /// </summary>
        /// <param name="updateable">The updateable object.</param>
        /// <param name="job">The job.</param>
        /// <param name="field">The field.</param>
        private static async void StoreUpdatable(
            IUpdateable updateable,
            OperationJob job,
            string field)
        {
            // Loop until the object has finished updating.
            // Every time the object updates, request a job update.
            while (await updateable.UpdateAsync()) await job.OnUpdate(job);
        }

        /// <summary>
        /// Handles the execution and cleanup of a job such as storing special results. 
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private async void HandleJob(
            OperationJob job,
            CancellationToken cancellationToken)
        {
            // Setup a cancellation token for the job.
            CancellationTokenSource tokenSource = new();
            CancellationToken token = tokenSource.Token;

            // Setup the next job.
            BackgroundJobUpdater jobUpdater = new(_logger, _persistence);

            // TODO: We should probably monitor the operations that are currently active.
            // Perform performing the job
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            try
            {
                // Setup job information.
                job.CancellationToken = token;
                job.OnUpdate = jobUpdater.UpdateJob;

                // Execute the job.
                Task jobUpdateTask = jobUpdater.LoopAsync(token);
                Task jobTask = job.Operation.Perform(job);
                await jobTask;

                // Iterate through the job results and handle special results.
                foreach (KeyValuePair<string, object> pair in job.Output)
                {
                    // Prepare for reading in interfaces.
                    Type[] genericArguments;

                    // Deconstruct the pair.
                    string field = pair.Key;
                    object value = pair.Value;

                    // If the value is an enumerable graph component, we need to store it vertex-by-vertex.
                    if (value.GetType().ImplementsGenericInterface(typeof(IEnumerableComponent<,>), out genericArguments))
                    {
                        MethodInfo storeGraphMethod = typeof(BackgroundJobService)
                            .GetMethod(nameof(StoreGraph), BindingFlags.NonPublic | BindingFlags.Static);
                        storeGraphMethod.InvokeGenericFunc(genericArguments, value, job, field);
                    }

                    // If the value is a structured accumulator, we need to store it continuously as it updates.
                    if (value is IUpdateable updateable)
                        StoreUpdatable(updateable, job, field);
                }

                // Perform final persistence-layer changes to the job.
                tokenSource.Cancel();
                await jobUpdateTask;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unrecoverable operations error occurred.");
                tokenSource.Cancel();
            }
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background operations service started.");
            while (!cancellationToken.IsCancellationRequested)
            {
                OperationJob job = await _jobQueue.Pop(cancellationToken);
                HandleJob(job, cancellationToken);
            }
            _logger.LogInformation("Background operations service stopping.");
        }
    }

    /// <summary>
    /// A helper class that manages updating job information for a particular operation continuously.
    /// </summary>
    public class BackgroundJobUpdater
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly Persistence _persistence;

        /// <summary>
        /// A task source that provides a method of waiting for an update to a job.
        /// </summary>
        public TaskCompletionSource JobUpdateFlag { get; private set; } = new();
        /// <summary>
        /// The current state of the operation job.
        /// </summary>
        public OperationJob Job { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobUpdater"/> class.
        /// </summary>
        public BackgroundJobUpdater(
            ILogger<BackgroundJobService> logger,
            Persistence persistence)
        {
            _logger = logger;
            _persistence = persistence;
        }

        /// <summary>
        /// Loops asynchronously waiting for updates to the job to process.
        /// Will wait until cancellation is requested. 
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task LoopAsync(CancellationToken cancellationToken)
        {
            // Continue updating the job until the cancellation token is cancelled.
            // Notice that we use a combination of a flag and persistence to batch updates to the job as quickly as
            // possible without race conditions while maintaining the ability to idle when no updates are needed.
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for an update to occur on the job.
                await JobUpdateFlag.Task;
                JobUpdateFlag = new TaskCompletionSource();

                // TODO: We will likely need to make this section more asynchronous to handle pipelined/etc. results.
                // Persist the job information.
                try
                {
                    JobItem jobItem = new(Job);
                    await OperationsController.SaveJobAsync(jobItem, _persistence);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error occurred while updating job information.");
                }
            }

            // Perform a final persistence update.
            // We do this in case of a race condition between a result and the persistence layer.
            try
            {
                JobItem jobItem = new(Job);
                await OperationsController.SaveJobAsync(jobItem, _persistence);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred while updating job information.");
            }
        }

        /// <summary>
        /// Updates the job information.
        /// This can be used as a dispatch method for each operation job.
        /// </summary>
        /// <param name="job">The job to use to update information.</param>
        public Task UpdateJob(OperationJob job)
        {
            Task task = JobUpdateFlag.Task;
            Job = job;
            JobUpdateFlag.TrySetResult();
            return task;
        }
    }
}