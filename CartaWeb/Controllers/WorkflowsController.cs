using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CartaCore.Operations;
using CartaCore.Persistence;
using CartaWeb.Models.DocumentItem;
using System.Linq;
using System;

namespace CartaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController : ControllerBase
    {
        // TODO: Logging.
        private readonly ILogger<WorkflowsController> _logger;
        private readonly Persistence _persistence;

        // public static async Task<OperationDescription[]> LoadWorkflowDescriptionsAsync()
        // {
        //     // Get all of the workflows.
        //     WorkflowItem[] workflowItems = await LoadWorkflowsAsync();

        //     // Convert the workflow items into descriptions.
        //     return workflowItems
        //         .Select(workflowItem =>
        //         {
        //             // TODO: We should make the "workflow" type a constant somewhere.
        //             // TODO: Allow tags to be assigned to workflows in front-end and back-end.
        //             // TODO: Allow description of the workflow to be assigned in front-end.
        //             return new OperationDescription()
        //             {
        //                 Type = "workflow",
        //                 Subtype = workflowItem.Id.ToString(),

        //                 Display = workflowItem.Name,
        //                 Description = workflowItem.Description,
        //                 Tags = Array.Empty<string>(),
        //             };
        //         })
        //         .ToArray();
        // }

        /// <inheritdoc />
        public WorkflowsController(ILogger<WorkflowsController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        #region Persistance
        // public static async Task<bool> SaveWorkflowAsync(WorkflowItem workflowItem, Persistence _persistence)
        // {

        // }
        // public static async Task<bool> DeleteWorkflowAsync(string workflowId, Persistence _persistence)
        // {

        // }
        public static async Task<WorkflowItem> LoadWorkflowAsync(string workflowId, Persistence _persistence)
        {
            WorkflowItem workflowItem = new WorkflowItem(workflowId);
            Item item = await _persistence.LoadItemAsync(workflowItem);
            return (WorkflowItem)item;
        }
        #endregion

        // #region Workflow CRUD
        // [HttpGet("{workflowId}")]
        // public async Task<ActionResult<WorkflowItem>> GetWorkflow(
        //     [FromRoute] string workflowId
        // )
        // {

        // }
        // [HttpPost]
        // public async Task<ActionResult<WorkflowItem>> CreateWorkflow(
        //     [FromBody] WorkflowItem workflowItem
        // )
        // {

        // }
        // [HttpPatch("workflowId")]
        // public async Task<ActionResult<WorkflowItem>> UpdateWorkflow(
        //     [FromRoute] string workflowId,
        //     [FromBody] WorkflowItem mergingWorkflowItem
        // )
        // {

        // }
        // [HttpDelete]
        // public async Task<ActionResult> DeleteWorkflow(
        //     [FromRoute] string workflowId
        // )
        // {

        // }
        // #endregion

        // #region Suboperations CRUD
        // [HttpGet("{workflowId}/operations")]
        // public async Task<ActionResult<OperationItem[]>> GetWorkflowOperations(
        //     [FromRoute] string workflowId
        // )
        // {

        // }
        // [HttpPost("{workflowId}/operations/{suboperationId}")]
        // public async Task<ActionResult> AddWorkflowOperation(
        //     [FromRoute] string workflowId,
        //     [FromRoute] string suboperationId
        // )
        // {

        // }
        // [HttpDelete("{workflowId}/operations/{suboperationId}")]
        // public async Task<ActionResult> RemoveWorkflowOperation(
        //     [FromRoute] string workflowId,
        //     [FromRoute] string suboperationId
        // )
        // {

        // }
        // #endregion
        
        // #region Connections CRUD
        // [HttpGet("{workflowId}/connections")]
        // public async Task<ActionResult<WorkflowConnection[]>> GetWorkflowConnections(
        //     [FromRoute] string workflowId
        // )
        // {

        // }
        // [HttpPost("{workflowId}/connections")]
        // public async Task<ActionResult<WorkflowConnection>> AddWorkflowConnection(
        //     [FromRoute] string workflowId,
        //     [FromBody] WorkflowConnection connection
        // )
        // {

        // }
        // [HttpPatch("{workflowId}/connections/{connectionId}")]
        // public async Task<ActionResult<WorkflowConnection>> UpdateWorkflowConnection(
        //     [FromRoute] string workflowId,
        //     [FromRoute] string connectionId,
        //     [FromBody] WorkflowConnection mergingConnection
        // )
        // {

        // }
        // [HttpDelete("{workflowId}/connections/{connectionId}")]
        // public async Task<ActionResult> DeleteWorkflowConnection(
        //     [FromRoute] string workflowId,
        //     [FromRoute] string connectionId
        // )
        // {

        // }

        // [HttpPost("{workflowId}/connections/suggest")]
        // public async Task<ActionResult<WorkflowConnection[]>> SuggestWorkflowConnections(
        //     [FromRoute] string workflowId,
        //     [FromBody] WorkflowConnection connection
        // )
        // {

        // }
        // #endregion
    }
}