using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CartaCore.Operations;
using CartaCore.Persistence;
using CartaWeb.Models.DocumentItem;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CartaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController : ControllerBase
    {
        // TODO: Logging.
        private readonly ILogger<WorkflowsController> _logger;
        private readonly Persistence _persistence;

        public static async Task<OperationDescription[]> LoadWorkflowDescriptionsAsync(Persistence _persistence)
        {
            // Get all of the workflows.
            List<WorkflowItem> workflowItems = await LoadWorkflowsAsync(_persistence);

            // Convert the workflow items into descriptions.
            return workflowItems
                .Select(workflowItem =>
                {
                    // TODO: We should make the "workflow" type a constant somewhere.
                    // TODO: Allow tags to be assigned to workflows in front-end and back-end.
                    // TODO: Allow description of the workflow to be assigned in front-end.
                    return new OperationDescription()
                    {
                        Type = "workflow",
                        Subtype = workflowItem.Id.ToString(),

                        Display = workflowItem.Name,
                        Description = workflowItem.Description,
                        Tags = Array.Empty<string>(),
                    };
                })
                .ToArray();
        }

        /// <inheritdoc />
        public WorkflowsController(ILogger<WorkflowsController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        #region Persistance
        public static async Task<bool> SaveWorkflowAsync(WorkflowItem workflowItem, Persistence _persistence)
        {
            DbDocument document = workflowItem.CreateDbDocument();
            return await _persistence.WriteDbDocumentAsync(document);
        }
        public static async Task<bool> UpdateWorkflowAsync(WorkflowItem workflowItem, Persistence _persistence)
        {
            DbDocument document = workflowItem.UpdateDbDocument();
            return await _persistence.WriteDbDocumentAsync(document);
        }
        public static async Task<bool> DeleteWorkflowAsync(string workflowId, Persistence _persistence)
        {
            WorkflowItem workflowItem = new WorkflowItem(workflowId);
            DbDocument document = workflowItem.DeleteDbDocument();
            return await _persistence.WriteDbDocumentAsync(document);
        }
        public static async Task<WorkflowItem> LoadWorkflowAsync(string workflowId, Persistence _persistence)
        {
            WorkflowItem workflowItem = new WorkflowItem(workflowId);
            Item item = await _persistence.LoadItemAsync(workflowItem);
            return (WorkflowItem)item;
        }
        public static async Task<List<WorkflowItem>> LoadWorkflowsAsync(Persistence _persistence)
        {
            WorkflowItem workflowItem = new WorkflowItem();
            IEnumerable<Item> items = await _persistence.LoadItemsAsync(workflowItem);
            return items.Select(item => (WorkflowItem)item).ToList();
        }
        #endregion

        #region Workflow CRUD
        [HttpPost]
        public async Task<ActionResult<WorkflowItem>> CreateWorkflow(
            [FromBody] WorkflowItem workflowItem)
        {
            // Set an identifier and make sure that internal representation fields have been set.
            // We make sure that the identifier has not already been set by the user otherwise, this is a bad request.
            // We make sure that the name is specified so that we have something to display and return from endpoints.
            if (workflowItem.Id is not null)
            {
                return BadRequest(new
                {
                    Error = "Workflow identifier should not be specified.",
                    Parameter = nameof(WorkflowItem.Id)
                });
            }
            if (workflowItem.Name is null)
            {
                return BadRequest(new
                {
                    Error = "Workflow name was expected but not specified.",
                    Parameter = nameof(WorkflowItem.Name)
                });
            }
            workflowItem.Id = NUlid.Ulid.NewUlid().ToString();
            workflowItem.Operations ??= Array.Empty<string>();
            workflowItem.Connections ??= Array.Empty<WorkflowConnection>();

            // Save the created workflow item and return its internal representation.
            await SaveWorkflowAsync(workflowItem, _persistence);
            return Ok(workflowItem);
        }
        [HttpGet("{workflowId}")]
        public async Task<ActionResult<WorkflowItem>> GetWorkflow(
            [FromRoute] string workflowId)
        {
            // Get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            return Ok(workflowItem);
        }
        [HttpPatch("{workflowId}")]
        public async Task<ActionResult<WorkflowItem>> UpdateWorkflow(
            [FromRoute] string workflowId,
            [FromBody] WorkflowItem mergingWorkflowItem)
        {
            // Get the existing workflow item and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We make sure that the identifier has not been set by the user otherwise, this is a bad request.
            // We make sure that the name is specified so that we have something to display and return from endpoints.
            if (mergingWorkflowItem.Id is not null && mergingWorkflowItem != workflowItem)
            {
                return BadRequest(new
                {
                    Error = "Workflow identifier cannot be modified.",
                    Parameter = nameof(WorkflowItem.Id)
                });
            }

            // We merge together the existing and merging workflow items.
            mergingWorkflowItem.Id = workflowItem.Id;
            mergingWorkflowItem.Name ??= workflowItem.Name;
            mergingWorkflowItem.Description ??= workflowItem.Description;
            mergingWorkflowItem.Operations ??= workflowItem.Operations;
            mergingWorkflowItem.Connections ??= workflowItem.Connections;

            // Save the updated workflow item and return its internal representation.
            await UpdateWorkflowAsync(workflowItem, _persistence);
            return Ok(workflowItem);
        }
        [HttpDelete("{workflowId}")]
        public async Task<ActionResult> DeleteWorkflow(string workflowId)
        {
            // Try to delete the workflow and return a status indicating whether successful.
            bool success = await DeleteWorkflowAsync(workflowId, _persistence);
            if (!success)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }
            else return Ok();
        }
        #endregion

        #region Suboperations CRUD
        [HttpGet("{workflowId}/operations")]
        public async Task<ActionResult<OperationItem[]>> GetWorkflowOperations(
            [FromRoute] string workflowId)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We retrieve each of the suboperations in the workflow from their identifiers.
            List<OperationItem> suboperations = new();
            foreach (string suboperationGuid in workflowItem.Operations)
                suboperations.Add(await OperationsController.LoadOperationAsync(suboperationGuid, _persistence));
            return suboperations.ToArray();
        }
        [HttpPost("{workflowId}/operations/{suboperationId}")]
        public async Task<ActionResult> AddWorkflowOperation(
            [FromRoute] string workflowId,
            [FromRoute] string suboperationId)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We get the suboperation from the store and check that it exists.
            OperationItem suboperationItem = await OperationsController.LoadOperationAsync(suboperationId, _persistence);
            if (suboperationItem is null)
            {
                return NotFound(new
                {
                    Error = "Suboperation with specified identifier could not be found.",
                    Id = suboperationId
                });
            }

            // We can now add the suboperation to the workflow and save it.
            workflowItem.Operations = workflowItem.Operations
                .Append(suboperationItem.Id)
                .ToArray();
            await UpdateWorkflowAsync(workflowItem, _persistence);
            return Ok();
        }
        [HttpDelete("{workflowId}/operations/{suboperationId}")]
        public async Task<ActionResult> RemoveWorkflowOperation(
            [FromRoute] string workflowId,
            [FromRoute] string suboperationId)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // Check that the operation that we are trying remove exists.
            if (!workflowItem.Operations.Contains(suboperationId))
            {
                return NotFound(new
                {
                    Error = "Suboperation with specified identifier could not be found.",
                    Id = suboperationId
                });
            }

            // We can now remove the suboperation from the workflow and save it.
            workflowItem.Operations = workflowItem.Operations
                .Where(operationId => operationId != suboperationId)
                .ToArray();

            // We also remove all references to the suboperation in the connections.
            for (int k = workflowItem.Connections.Length - 1; k >= 0; k--)
            {
                WorkflowConnection connectionItem = workflowItem.Connections[k];
                if (
                    connectionItem.Source.Operation == suboperationId ||
                    connectionItem.Target.Operation == suboperationId
                )
                {
                    List<WorkflowConnection> connections = workflowItem.Connections.ToList();
                    connections.RemoveAt(k);
                    workflowItem.Connections = connections.ToArray();
                }
            }

            await UpdateWorkflowAsync(workflowItem, _persistence);
            return Ok();
        }
        #endregion

        #region Connections CRUD
        [HttpGet("{workflowId}/connections")]
        public async Task<ActionResult<WorkflowConnection[]>> GetWorkflowConnections(
            [FromRoute] string workflowId)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We return the workflow connections.
            return Ok(workflowItem.Connections);
        }
        [HttpPost("{workflowId}/connections")]
        public async Task<ActionResult<WorkflowConnection>> AddWorkflowConnection(
            [FromRoute] string workflowId,
            [FromBody] WorkflowConnection connection)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We make sure that the identifier has not already been set by the user otherwise, this is a bad request.
            if (connection.Id is not null)
            {
                return BadRequest(new
                {
                    Error = "Connection identifier should not be specified.",
                    Parameter = nameof(WorkflowConnection.Id)
                });
            }
            connection.Id = NUlid.Ulid.NewUlid().ToString();

            // We must check if the operations referenced in the source and target points are contained within the
            // workflow itself. If either is not contained in the workflow, we return not found.
            if (!workflowItem.Operations.Contains(connection.Source.Operation))
            {
                return NotFound(new
                {
                    Error = "Source operation specified in connection could not be found.",
                    Id = connection.Source.Operation
                });
            }
            if (!workflowItem.Operations.Contains(connection.Target.Operation))
            {
                return NotFound(new
                {
                    Error = "Target operation specified in connection could not be found.",
                    Id = connection.Target.Operation
                });
            }

            // We must check that there is not already a connection that has the same target operation and field.
            if (
                workflowItem.Connections
                .Any(existing =>
                    existing.Target.Operation == connection.Target.Operation &&
                    string.Equals(existing.Target.Field, connection.Target.Field, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return Conflict(new
                {
                    Error = "Connection specifies target field that is already connected to.",
                    Id = connection.Target.Operation,
                    Field = connection.Target.Field
                });
            }

            // TODO: We must check if the fields specified on the connection source and target points are valid for
            //       the type of operations they reference.

            // We add the connection and return it.
            workflowItem.Connections = workflowItem.Connections
                .Append(connection)
                .ToArray();
            await UpdateWorkflowAsync(workflowItem, _persistence);
            return connection;
        }
        [HttpPatch("{workflowId}/connections/{connectionId}")]
        public async Task<ActionResult<WorkflowConnection>> UpdateWorkflowConnection(
            [FromRoute] string workflowId,
            [FromRoute] string connectionId,
            [FromBody] WorkflowConnection mergingConnection)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We return "Not Found (404)" if the connection does not exist.
            int connectionIndex = Array
                .FindIndex(workflowItem.Connections, connection => connection.Id == connectionId);
            if (connectionIndex < 0)
            {
                return NotFound(new
                {
                    Error = "Connection with specified identifier could not be found.",
                    Id = connectionId
                });
            }
            WorkflowConnection connection = workflowItem.Connections[connectionIndex];

            // We must check that no identifying or structure-based properties have not been changes. 
            if (mergingConnection.Id is not null && mergingConnection.Id != connection.Id)
            {
                return BadRequest(new
                {
                    Error = "Connection identifier cannot be modified.",
                    Parameter = nameof(WorkflowConnection.Id)
                });
            }
            if (mergingConnection.Source is not null)
            {
                if (mergingConnection.Source.Operation != connection.Source.Operation)
                {
                    return BadRequest(new
                    {
                        Error = "Connection source operation cannot be modified.",
                        Parameter = nameof(WorkflowConnection.Source)
                    });
                }
                if (mergingConnection.Source.Field != connection.Source.Field)
                {
                    return BadRequest(new
                    {
                        Error = "Connection source field cannot be modified.",
                        Parameter = nameof(WorkflowConnection.Source)
                    });
                }
            }
            if (mergingConnection.Target is not null)
            {
                if (mergingConnection.Target.Operation != connection.Target.Operation)
                {
                    return BadRequest(new
                    {
                        Error = "Connection target operation cannot be modified.",
                        Parameter = nameof(WorkflowConnection.Target)
                    });
                }
                if (mergingConnection.Target.Field != connection.Target.Field)
                {
                    return BadRequest(new
                    {
                        Error = "Connection target field cannot be modified.",
                        Parameter = nameof(WorkflowConnection.Target)
                    });
                }
            }

            // Merge the existing and merging connections together and save.
            mergingConnection.Id = connection.Id;
            mergingConnection.Source = connection.Source;
            mergingConnection.Target = connection.Target;
            workflowItem.Connections[connectionIndex] = mergingConnection;
            await UpdateWorkflowAsync(workflowItem, _persistence);
            return connection;
        }
        [HttpDelete("{workflowId}/connections/{connectionId}")]
        public async Task<ActionResult> RemoveWorkflowConnection(
            [FromRoute] string workflowId,
            [FromRoute] string connectionId)
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // Check that the connection that we are trying remove exists.
            if (!workflowItem.Connections.Any(connection => connection.Id == connectionId))
            {
                return NotFound(new
                {
                    Error = "Connection with specified identifier could not be found.",
                    Id = connectionId
                });
            }

            // Remove the connection and save the workflow.
            workflowItem.Connections = workflowItem.Connections
                .Where(connection => connection.Id != connectionId)
                .ToArray();
            await UpdateWorkflowAsync(workflowItem, _persistence);
            return Ok();
        }

        [HttpPost("{workflowId}/connections/suggest")]
        public async Task<ActionResult<WorkflowConnection>> GetWorkflowConnectionSuggestion(
            [FromRoute] string workflowId,
            [FromBody] WorkflowConnection connection
        )
        {
            // We try to get the workflow from the store and check that it exists.
            WorkflowItem workflowItem = await LoadWorkflowAsync(workflowId, _persistence);
            if (workflowItem is null)
            {
                return NotFound(new
                {
                    Error = "Workflow with specified identifier could not be found.",
                    Id = workflowId
                });
            }

            // We must check if the operations referenced in the source and target points are contained within the
            // workflow itself. If either is not contained in the workflow, we return not found.
            if (!workflowItem.Operations.Contains(connection.Source.Operation))
            {
                return NotFound(new
                {
                    Error = "Source operation specified in connection could not be found.",
                    Id = connection.Source.Operation
                });
            }
            if (!workflowItem.Operations.Contains(connection.Target.Operation))
            {
                return NotFound(new
                {
                    Error = "Target operation specified in connection could not be found.",
                    Id = connection.Target.Operation
                });
            }

            // TODO: Generalize to workflows.
            OperationItem sourceOperation = await OperationsController.LoadOperationAsync
            (
                connection.Source.Operation.ToString(),
                _persistence
            );
            OperationItem targetOperation = await OperationsController.LoadOperationAsync
            (
                connection.Target.Operation.ToString(),
                _persistence
            );

            WorkflowOperation workflow = (WorkflowOperation)await OperationsController.InstantiateOperation(new OperationItem()
            {
                Type = "workflow",
                Subtype = workflowId,
                Id = null
            }, _persistence);
            if (workflow.IsMultiplexLike(connection.Target.Operation, new WorkflowOperationConnection()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = connection.Source.Operation,
                    Field = connection.Source.Field
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = connection.Target.Operation,
                    Field = connection.Target.Field
                },
                Multiplexer = connection.Multiplex
            }))
                connection.Multiplex = true;


            // TODO: Implement
            // TODO: Tells if passed in connection is able to be made valid. ("valid" boolean field).
            return connection;
        }
        #endregion
    }
}