using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CartaCore.Serialization.Json;
using CartaCore.Workflow;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Manages creation, retrieval, updating, and deletion of workflow and workflow operation objects. The workflow
    /// operations are selector-action pairs. More information on the structure of selectors and actions can be
    /// retrieved from the workflow API. Workflows have certain user permissions associated with them allowing only
    /// certain users access to specific workflows. Each workflow is automatically assigned an identifier by the server.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : ControllerBase
    {
        /// <summary>
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<WorkflowController> _logger;

        /// <inheritdoc />
        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _logger = logger;
        }

        private static string BaseDirectory = @"data";
        private static string WorkflowDirectory = @"workflows";

        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        static WorkflowController()
        {
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.Converters.Insert(0, new DiscriminantConverter());
        }

        /// <summary>
        /// Loads all of the stored workflows asynchronously.
        /// </summary>
        /// <returns>A list of all stored workflows.</returns>
        public static async Task<List<Workflow>> LoadWorkflowsAsync()
        {
            // Read all of the files.
            string workflowDir = Path.Combine(BaseDirectory, WorkflowDirectory);
            if (!Directory.Exists(workflowDir)) return new List<Workflow>();
            return await Directory.GetFiles(workflowDir)
                .Select(filePath => int.Parse(Path.GetFileNameWithoutExtension(filePath)))
                .ToAsyncEnumerable()
                .SelectAwait(async id => await LoadWorkflowAsync(id))
                .ToListAsync();
        }
        /// <summary>
        /// Loads a single workflow asynchronously specified by an identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        public static async Task<Workflow> LoadWorkflowAsync(int id)
        {
            // Try to read the JSON file.
            string workflowPath = Path.Combine(BaseDirectory, WorkflowDirectory, $"{id}.json");
            if (!System.IO.File.Exists(workflowPath)) return null;
            using (StreamReader file = new StreamReader(workflowPath))
            {
                Workflow workflow = JsonSerializer.Deserialize<Workflow>(await file.ReadToEndAsync(), JsonOptions);
                workflow.Id = id;
                return workflow;
            }
        }
        /// <summary>
        /// Saves a single workflow asynchronously. If an existing identifier is specified, the workflow is overwritten.
        /// Otherwise, an unused identifier is chosen and the workflow is saved there.
        /// </summary>
        /// <param name="workflow">The workflow object.</param>
        /// <param name="existingId">An existing workflow identifier, if applicable.</param>
        /// <returns>The identifier of the saved workflow.</returns>
        public static async Task<int> SaveWorkflowAsync(Workflow workflow, int? existingId = null)
        {
            // Make sure that the operations list exists.
            workflow.Operations = workflow.Operations ?? new List<WorkflowOperation>();

            // Find out what identifier we should assign to this workflow.
            string workflowDir = Path.Combine(BaseDirectory, WorkflowDirectory);
            Directory.CreateDirectory(workflowDir);
            int id = 0;
            if (existingId.HasValue) id = existingId.Value;
            else while (System.IO.File.Exists(Path.Combine(workflowDir, $"{id}.json"))) id++;

            // Create the JSON file.
            string workflowPath = Path.Combine(workflowDir, $"{id}.json");
            using (StreamWriter file = new StreamWriter(workflowPath))
            {
                string json = JsonSerializer.Serialize<Workflow>(workflow, JsonOptions);
                await file.WriteAsync(json);
            }

            // Return the identifier of the created workflow.
            return id;
        }
        /// <summary>
        /// Updates a single workflow asynchronously specified by an identifier by merging in another workflow and its
        /// properties.
        /// </summary>
        /// <param name="id">The identifier of the workflow to update.</param>
        /// <param name="workflow">The workflow whose properties should be merged in.</param>
        /// <returns>The updated workflow.</returns>
        public static async Task<Workflow> UpdateWorkflowAsync(int id, Workflow workflow)
        {
            // We get the stored workflow first so we can perform updates on it.
            Workflow storedWorkflow = await LoadWorkflowAsync(id);

            if (storedWorkflow is null)
                // The stored workflow does not exist so we just assign the entire object.
                storedWorkflow = workflow;
            else
            {
                // Copy over all the properties if specified on the updating object.
                storedWorkflow.Name = workflow.Name ?? storedWorkflow.Name;
                storedWorkflow.Operations = workflow.Operations ?? storedWorkflow.Operations;
            }
            await SaveWorkflowAsync(workflow, id);
            return storedWorkflow;
        }
        /// <summary>
        /// Deletes a single workflow specified by an identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow to delete.</param>
        public static async Task<bool> DeleteWorkflowAsync(int id)
        {
            // Try to delete the JSON file.
            string workflowPath = Path.Combine(BaseDirectory, WorkflowDirectory, $"{id}.json");
            if (System.IO.File.Exists(workflowPath))
            {
                System.IO.File.Delete(workflowPath);
                return await Task.FromResult(true);
            }
            else return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a list of workflow objects that the user has access to. This may contain workflow objects that the user
        /// has created or that have been given access to by another user.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">A list of accessible workflow objects.</returns>
        [HttpGet]
        public async Task<ActionResult<List<Workflow>>> GetWorkflows()
        {
            return Ok(await LoadWorkflowsAsync());
        }
        /// <summary>
        /// Gets a workflow object specified by an identifier. The client must have the appropriate access permissions
        /// to access the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">0</arg>
        /// </request>
        /// <returns status="200">The workflow object with the specified identifier.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Workflow>> GetWorkflow(
            [FromRoute] int id
        )
        {
            Workflow workflow = await LoadWorkflowAsync(id);
            if (workflow is null) return NotFound();
            else return Ok(workflow);
        }
        /// <summary>
        /// Creates a new workflow object. The user will initially have all permission access to this workflow. If
        /// properties of the workflow are not specified, they will be filled in with defaults.
        /// </summary>
        /// <param name="workflow">The workflow to create.</param>
        /// <request name="Empty">
        ///     <body>{}</body>
        /// </request>
        /// <request name="Named">
        ///     <body>
        ///     {
        ///         "name": "Example New Workflow"
        ///     }
        ///     </body>
        /// </request>
        /// <request name="With Operations">
        ///     <body>
        ///     {
        ///         "name": "Example New Workflow with Operations",
        ///         "operations":
        ///         [
        ///             {
        ///                 "selector": { "type": "all" },
        ///                 "action": { "type": "increment", "amount": 5 }
        ///             },
        ///             {
        ///                 "selector":
        ///                 {
        ///                     "type": "property range",
        ///                     "property": "foo",
        ///                     "maximum": 10
        ///                 },
        ///                 "action": { "type": "increment" }
        ///             }
        ///         ]
        ///     }    
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow object that was created. An automatically generated identifier will be attached to the returned
        /// object.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<Workflow>> PostWorkflow(
            [FromBody] Workflow workflow
        )
        {
            int id = await SaveWorkflowAsync(workflow);
            workflow.Id = id;
            return Ok(workflow);
        }
        /// <summary>
        /// Updates an existing workflow object specified by an identifier. The client must have the appropriate access
        /// permissions to access the workflow. Any properties that are specified in the update will overwrite existing
        /// properties. If a property is not specified, it is not updated in the original object.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="workflow">The workflow to use to update the existing workflow.</param>
        /// <request name="Name Only">
        ///     <arg name="id">0</arg>
        ///     <body>
        ///     {
        ///         "name": "Updated Workflow Name"
        ///     }    
        ///     </body>
        /// </request>
        /// <request name="Clear Operations">
        ///     <arg name="id">1</arg>
        ///     <body>
        ///     {
        ///         "operations": []
        ///     }    
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow object after updates are applied. The workflow identifier will be attached to the returned
        /// object.
        /// </returns>
        [HttpPatch("{id}")]
        public async Task<ActionResult<Workflow>> PatchWorkflow(
            [FromRoute] int id,
            [FromBody] Workflow workflow
        )
        {
            workflow = await UpdateWorkflowAsync(id, workflow);
            return Ok(workflow);
        }
        /// <summary>
        /// Deletes an existing workflow object specified by an identifier if it exists. The client must have the
        /// appropriate access permissions to access the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">0</arg>
        /// </request>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkflow(
            [FromRoute] int id
        )
        {
            if (await DeleteWorkflowAsync(id))
                return Ok();
            else
                return NotFound();
        }

        /// <summary>
        /// Gets the list of operations from a workflow object specified by an identifier. The client must have the
        /// appropriate access permissions to access the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">0</arg>
        /// </request>
        /// <returns status="200">The list of operations attached to the workflow.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [HttpGet("{id}/operations")]
        public async Task<ActionResult<List<WorkflowOperation>>> GetWorkflowOperations(
            [FromRoute] int id
        )
        {
            Workflow workflow = await LoadWorkflowAsync(id);
            if (workflow is null) return NotFound();
            else return Ok(workflow.Operations);
        }
        /// <summary>
        /// Gets an operation specified by index from a workflow object specified by an identifier. The client must have
        /// the appropriate access permissions to access the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index of the workflow operation.</param>
        /// <request name="First">
        ///     <arg name="id">0</arg>
        ///     <arg name="index">0</arg>
        /// </request>
        /// <request name="Last">
        ///     <arg name="id">0</arg>
        ///     <arg name="index">-1</arg>
        /// </request>
        /// <returns status="200">The specified operation attached to the workflow.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier or there is no workflow operation
        /// corresponding to the specified index.
        /// </returns>
        [HttpGet("{id}/operations/{index}")]
        public async Task<ActionResult<WorkflowOperation>> GetWorkflowOperation(
            [FromRoute] int id,
            [FromRoute] int index
        )
        {
            // Get the workflow.
            Workflow workflow = await LoadWorkflowAsync(id);
            if (workflow is null) return NotFound();

            // Convert the index and get the operation.
            if (workflow.Operations is null ||
                workflow.Operations.Count == 0
            ) return NotFound();
            if (index < 0) index += workflow.Operations.Count;
            if (index < 0 || index >= workflow.Operations.Count) return NotFound();
            return Ok(workflow.Operations[index]);
        }
        /// <summary>
        /// Inserts a new workflow operation at the optionally specified index into the list of operations of a workflow
        /// object specified by an identifier. The client must have the appropriate access permissions to access the
        /// workflow. If the index of the operation is not specified, the operation appends the new operation at the end
        /// of the list of operations. If properties of the operation are not specified, they will be filled in with
        /// defaults.
        /// </summary>
        /// <param name="id">The identifier of the workflow</param>
        /// <param name="index">The index to insert the workflow operation at.</param>
        /// <param name="operation">The operation to insert.</param>
        /// <request name="Append">
        ///     <arg name="id">0</arg>
        ///     <body>
        ///     {
        ///         "action":    
        ///         {
        ///             "type": "decrement"
        ///         }
        ///     }    
        ///     </body>
        /// </request>
        /// <request name="Insert Second">
        ///     <arg name="id">0</arg>
        ///     <arg name="index">1</arg>
        ///     <body>
        ///     {
        ///         "name": "My New Operation",    
        ///         "selector":
        ///         {
        ///             "type": "vertex name",
        ///             "pattern": "bar"
        ///         },
        ///         "action":
        ///         {    
        ///             "type": "decrement",
        ///             "amount": 2
        ///         }
        ///     }
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow operation object that was created.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the index is out of range of the list of operations.
        /// </returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [HttpPost("{id}/operations/{index?}")]
        public async Task<ActionResult<WorkflowOperation>> InsertWorkflowOperation(
            [FromRoute] int id,
            [FromRoute] int? index,
            [FromBody] WorkflowOperation operation
        )
        {
            // Get the workflow.
            Workflow workflow = await LoadWorkflowAsync(id);
            if (workflow is null) return NotFound();

            // Try to perform the insertion.
            if (workflow.Operations is null) workflow.Operations = new List<WorkflowOperation>();
            if (index.HasValue)
            {
                if (index < 0) index += workflow.Operations.Count;
                if (index < 0 || index > workflow.Operations.Count) return BadRequest();
                workflow.Operations.Insert(index.Value, operation);
            }
            else workflow.Operations.Add(operation);

            // Save the changes to the workflow.
            await UpdateWorkflowAsync(id, workflow);
            return Ok(operation);
        }
        /// <summary>
        /// Updates an existing workflow operation specified by an index in the list of operations of a workflow
        /// object specified by an identifier. The client must have the appropriate access permissions to access the
        /// workflow. Any properties that are specified in the update will overwrite existing properties. If a property
        /// is not specified, it is not updated in the original object.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index to update the workflow operation at.</param>
        /// <param name="operation">The operation to use to update the existing operation.</param>
        /// <request name="Update First with Name">
        ///     <arg name="id">0</arg>
        ///     <arg name="index">0</arg>
        ///     <body>
        ///     {
        ///         "name": "Updated Operation Name"
        ///     }
        ///     </body>
        /// </request>
        /// <request name="Update Last with Selector">
        ///     <arg name="id">0</arg>
        ///     <arg name="index">-1</arg>
        ///     <body>
        ///     {
        ///         "selector": { "type": "none" }
        ///     }
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow operation object after updates are applied.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the index is out of range of the list of operations.
        /// </returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [HttpPatch("{id}/operations/{index}")]
        public async Task<ActionResult<WorkflowOperation>> PatchWorkflowOperation(
            [FromRoute] int id,
            [FromRoute] int index,
            [FromBody] WorkflowOperation operation
        )
        {
            // Get the workflow.
            Workflow workflow = await LoadWorkflowAsync(id);
            if (workflow is null) return NotFound();

            // Try to perform the update.
            if (workflow.Operations is null) return BadRequest();
            if (index < 0) index += workflow.Operations.Count;
            if (index < 0 || index >= workflow.Operations.Count) return BadRequest();

            workflow.Operations[index].Name = operation.Name ?? workflow.Operations[index].Name;
            workflow.Operations[index].Selector = operation.Selector ?? workflow.Operations[index].Selector;
            workflow.Operations[index].Action = operation.Action ?? workflow.Operations[index].Action;

            // Save the changes to the workflow.
            await UpdateWorkflowAsync(id, workflow);
            return Ok(workflow.Operations[index]);
        }
        /// <summary>
        /// Removes an operation optionally specified by index from a workflow object specified by identifier. The
        /// client must have appropriate access permissions to access the workflow. If the index of the operation is not
        /// specified, the operation removes the last operation in the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index of the workflow operation.</param>
        /// <request name="Remove Last">
        ///     <arg name="id">0</arg>
        /// </request>
        /// <request name="Remove Second">
        ///     <arg name="id">0</arg>
        ///     <arg name="index">1</arg>
        /// </request>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier or there is no workflow operation
        /// corresponding to the specified index.
        /// </returns>
        [HttpDelete("{id}/operations/{index?}")]
        public async Task<ActionResult> RemoveWorkflowOperation(
            [FromRoute] int id,
            [FromRoute] int? index
        )
        {
            // Get the workflow.
            Workflow workflow = await LoadWorkflowAsync(id);
            if (workflow is null) return NotFound();

            // Try to perform the deletion.
            if (workflow.Operations is null) return BadRequest();
            if (index.HasValue)
            {
                if (index < 0) index += workflow.Operations.Count;
                if (index < 0 || index >= workflow.Operations.Count) return BadRequest();
                workflow.Operations.RemoveAt(index.Value);
            }
            else if (workflow.Operations.Count > 0)
                workflow.Operations.RemoveAt(workflow.Operations.Count - 1);

            // Save the changes to the workflow.
            await UpdateWorkflowAsync(id, workflow);
            return Ok();
        }
    }
}