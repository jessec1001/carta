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
        private BackgroundJobQueue TaskCollection;
        private ILogger<BackgroundJobService> Logger;
        private Persistence _persistence;
        private IServiceScopeFactory ServiceScopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobService"/> service.
        /// </summary>
        /// <param name="taskCollection"></param>
        /// <param name="logger"></param>
        /// <param name="noSqlDbContext"></param>
        /// <param name="serviceScopeFactory"></param>
        public BackgroundJobService(
            BackgroundJobQueue taskCollection,
            ILogger<BackgroundJobService> logger,
            INoSqlDbContext noSqlDbContext,
            IServiceScopeFactory serviceScopeFactory)
        {
            TaskCollection = taskCollection;
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
            _persistence = new Persistence(noSqlDbContext);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Background operations service started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                (JobItem jobItem, Operation operation, OperationJob job) = await TaskCollection.Pop();
                using IServiceScope scope = ServiceScopeFactory.CreateScope();
                try
                {
                    await operation.Perform(job);
                    jobItem.Completed = true;
                    jobItem.Result = job.Output;
                    await OperationsController.UpdateJobAsync(jobItem, _persistence);
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "Unrecoverable operations error occurred.");
                }
            }
            Logger.LogInformation("Background operations service stopping.");
        }
    }
}