using System;
using System.Threading;
using System.Threading.Tasks;
using CartaWeb.Controllers;
using CartaWeb.Models.DocumentItem;
using CartaCore.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CartaWeb.Services;
using CartaCore.Persistence;

namespace CartaWeb.Services
{
    // TODO: Add authentication information from endpoints to jobs passed to this service.
    // TODO: Implement something, perhaps in the task running service, that will automatically handle loading uploaded
    //       files into streams and handle saving streams to downloadable files.

    // TODO: We are running a single background job at a time. We should be able to run multiple background jobs at once.

    /// <summary>
    /// A service that is responsible for running operation jobs in the background across multiple threads.
    /// </summary>
    public class BackgroundJobService : BackgroundService
    {
        private BackgroundJobQueue _jobQueue;
        private ILogger<BackgroundJobService> _logger;
        private Persistence _persistence;
        private IServiceScopeFactory _serviceScopeFactory;

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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background operations service started.");
            while (!cancellationToken.IsCancellationRequested)
            {
                // Setup a cancellation token for the job.
                CancellationTokenSource tokenSource = new();
                CancellationToken token = tokenSource.Token;

                // Fetch the next job.
                OperationJob job = await _jobQueue.Pop(cancellationToken);
                job.CancellationToken = token;

                // TODO: We should probably monitor the operations that are currently active.
                // Perform performing the job
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                try
                {
                    Task jobTask = job.Operation.Perform(job);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unrecoverable operations error occurred.");
                }
                finally 
                {
                    tokenSource.Cancel();
                }
            }
            _logger.LogInformation("Background operations service stopping.");
        }
    }

    /// <summary>
    /// A helper class that manages updating job information for a particular operation continuously.
    /// </summary>
    public class BackgroundJobUpdater
    {
        private ILogger<BackgroundJobService> _logger;
        private Persistence _persistence;

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
                // TODO: We might need to make sure that the cancellation token does not cut off the final update.
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
        }

        /// <summary>
        /// Updates the job information.
        /// This can be used as a dispatch method for each operation job.
        /// </summary>
        /// <param name="job">The job to use to update information.</param>
        public Task UpdateJob(OperationJob job)
        {
            Job = job;
            JobUpdateFlag.TrySetResult();
            return JobUpdateFlag.Task;
        }
    }
}