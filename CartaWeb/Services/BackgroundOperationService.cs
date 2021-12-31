using System;
using System.Collections.Generic;
using System.IO;
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
                Func<IServiceScopeFactory, (JobItem jobItem, Operation operation, OperationContext context)> item = await TaskCollection.Pop();

                    (JobItem jobItem, Operation operation, OperationContext context) = item(ServiceScopeFactory);
                try
                {
                    if (operation is WorkflowOperation workflow)
                    {
                        foreach (string inputField in workflow.GetInputFields())
                        {
                            Type inputType = workflow.GetInputFieldType(inputField);
                            if (inputType.IsAssignableTo(typeof(Stream)) && context.Input.ContainsKey(inputField) && context.Input[inputField] is bool)
                            {
                                context.Input[inputField] = OperationsController.ReadFile();
                            }
                        }
                    }
                    await operation.Perform(context);
                   

                    jobItem.Completed = true;
                    jobItem.Result = context.Output;

                    await OperationsController.SaveJobAsync(jobItem, _persistence);
                }
                catch(Exception exception)
                {
                    Logger.LogError(exception, "Unrecoverable operations error occurred.");
                }
                finally
                {
                    foreach (KeyValuePair<string, object> entry in context.Input)
                    {
                        if (entry.Value is Stream stream)
                            stream.Close();
                    }
                }
            }
            Logger.LogInformation("Background operations service stopping.");
        }
    }
}