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

namespace Carta.Api.Services
{
    public class BackgroundOperationService : BackgroundService
    {
        private OperationTaskCollection TaskCollection;
        private ILogger<BackgroundOperationService> Logger;
        private Persistence _persistence;
        private IServiceScopeFactory ServiceScopeFactory;

        public BackgroundOperationService(OperationTaskCollection taskCollection, ILogger<BackgroundOperationService> logger, INoSqlDbContext noSqlDbContext, IServiceScopeFactory serviceScopeFactory)
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
                (JobItem jobItem, Operation operation, OperationContext context) = await TaskCollection.Pop();
                using IServiceScope scope = ServiceScopeFactory.CreateScope();
                try
                {
                    await operation.Perform(context);
                    jobItem.Completed = true;
                    jobItem.Result = context.Output;
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