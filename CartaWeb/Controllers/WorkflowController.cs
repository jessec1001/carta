using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CartaCore.Persistence;
using CartaCore.Workflow;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;

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

        /// <summary>
        /// The persistence object used to read and write from the database
        /// </summary>
        private readonly Persistence _persistence;

        /// <inheritdoc />
        public WorkflowController(ILogger<WorkflowController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        /// <summary>
        /// Returns the current version number of the workflow with the given identifier.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="persistence">The persistence class used to access the database.</param>
        /// <returns>
        /// The current version number.
        /// </returns>
        public static async Task<int> GetCurrentWorkflowVersionNumber(
            string userId,
            string id,
            Persistence persistence
        )
        {
            WorkflowAccessItem workflowAccessItem = new WorkflowAccessItem(true, userId, id);
            workflowAccessItem = (WorkflowAccessItem)await persistence.LoadItemAsync(workflowAccessItem);
            if (workflowAccessItem is null) return 0;
            else return workflowAccessItem.VersionInformation.Number;
        }

        /// <summary>
        /// Loads all of the stored workflows asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>A list of all stored workflows.</returns>
        protected async Task<List<Workflow>> LoadWorkflowsAsync(string userId)
        {
            List<Workflow> workflows = new() { };

            // Retrieve temporary working versions of workflows
            WorkflowItem readWorkflowItem = new WorkflowItem(true, userId);
            IEnumerable<Item> workflowItems = await _persistence.LoadItemsAsync(readWorkflowItem);
            foreach (WorkflowItem workflowItem in workflowItems)
            {
                workflowItem.Workflow.VersionNumber = workflowItem.VersionInformation.Number;
                workflows.Add(workflowItem.Workflow);
            }

            // Retrieve persisted workflows
            WorkflowAccessItem readWorkflowAccessItem = new WorkflowAccessItem(true, userId);
            IEnumerable<Item> WorkflowAccessItems =
                await _persistence.LoadItemsAsync(readWorkflowAccessItem);
            foreach (WorkflowAccessItem workflowAccessItem in WorkflowAccessItems)
            {
                WorkflowItem workflowItem = await LoadWorkflowAsync
                (
                    workflowAccessItem.Id,
                    workflowAccessItem.VersionInformation.Number,
                    _persistence
                );
                if (workflowItem != null) workflows.Add(workflowItem.Workflow);
            }

            return workflows;
        }

        /// <summary>
        /// Loads a single workflow asynchronously for the given workflow identifier.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="versionNumber">The version number of the workflow to get. If not specified, the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the current version of the workflow is returned.</param>
        /// <param name="persistence">The persistence class used to access the database.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        public static async Task<WorkflowItem> LoadWorkflowAsync(
            string userId,
            string id,
            int? versionNumber,
            Persistence persistence
        )
        {
            if (versionNumber.HasValue)
            {
                return await LoadWorkflowAsync(id, versionNumber.Value, persistence);
            }
            else
            {
                WorkflowItem workflowItem = await LoadTemporaryWorkflowAsync(userId, id, persistence);
                if (workflowItem is not null)
                {
                    Workflow workflow = workflowItem.Workflow;
                    workflow.Id = workflowItem.Id;
                    workflow.VersionNumber = workflowItem.VersionInformation.Number;
                    return workflowItem;
                }
                else
                {
                    int nr = await GetCurrentWorkflowVersionNumber(userId, id, persistence);
                    return await LoadWorkflowAsync(id, nr, persistence);
                }
            }
        }

        /// <summary>
        /// Loads a single workflow item asynchronously for the specified workflow identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="versionNumber">The version number of the workflow to get.</param>
        /// <param name="persistence">The persistence class used to access the database.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        public static async Task<WorkflowItem> LoadWorkflowAsync(
            string id,
            int versionNumber,
            Persistence persistence
        )
        {
            WorkflowItem workflowItem = new WorkflowItem(id, versionNumber);
            Item item = await persistence.LoadItemAsync(workflowItem);
            workflowItem = (WorkflowItem)item;
            if (workflowItem is not null) workflowItem.IsTempWorkflow = false;
            return workflowItem;
        }

        /// <summary>
        /// Loads the temporary working version of single workflow item.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="persistence">The persistence class used to access the database.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        protected static async Task<WorkflowItem> LoadTemporaryWorkflowAsync(
            string userId,
            string id,
            Persistence persistence
        )
        {
            WorkflowItem workflowItem = new WorkflowItem(userId, id);
            Item item = await persistence.LoadItemAsync(workflowItem);
            workflowItem = (WorkflowItem)item;
            if (workflowItem is not null) workflowItem.IsTempWorkflow = true;
            return workflowItem;
        }

        /// <summary>
        /// Retrieves version information of all versions available for the specified workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow</param>
        /// <returns>
        /// A list containing version information, ordered by decreasing version number
        /// </returns>
        protected async Task<List<VersionInformation>> LoadWorkflowVersionsAsync(string id)
        {
            List<VersionInformation> list = new() { };

            // Retrieve version information
            WorkflowItem readWorkflowItem = new WorkflowItem(false, id);
            IEnumerable<Item> workflowItems = await _persistence.LoadItemsAsync(readWorkflowItem);
            foreach (WorkflowItem workflowItem in workflowItems)
            {
                list.Add(workflowItem.VersionInformation);
            }

            // Sort the list by version number (decreasing)
            list.Sort((p, q) => q.Number.CompareTo(p.Number));

            return list;
        }


        /// <summary>
        /// Saves a workflow as a user's temporary workflow
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to update.</param>
        /// <param name="workflowItem">The workflowItem that should be persisted.</param>
        /// <returns>true if the workflow was updated successfully, else false.</returns>
        protected async Task<bool> SaveTemporaryWorkflowAsync(
            string userId,
            string id,
            WorkflowItem workflowItem)
        {
            WorkflowItem saveWorkflowItem = new WorkflowItem
            (
                true,
                userId,
                workflowItem.Workflow,
                workflowItem.VersionInformation
            );

            WorkflowAccessItem readWorkflowAccessItem = new WorkflowAccessItem(true, userId, id);
            Item readItem = await _persistence.LoadItemAsync(readWorkflowAccessItem);
            if (readItem is null)
            {
                return await _persistence.WriteDbDocumentAsync(saveWorkflowItem.SaveDbDocument());
            }
            else
            {
                // Also delete the workflow access item 
                WorkflowAccessItem deleteWorkflowAccessItem = new WorkflowAccessItem(true, userId, id);
                return await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
                {
                    saveWorkflowItem.SaveDbDocument(),
                    deleteWorkflowAccessItem.DeleteDbDocument()
                });
            }      
        }

        /// <summary>
        /// Gets a list of workflow objects that the user has access to. 
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">A list of accessible workflow objects.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Workflow>>> GetWorkflows()
        {
            return Ok(await LoadWorkflowsAsync(new UserInformation(User).Id));
        }

        /// <summary>
        /// Gets a workflow object specified by an identifier and optional version number. 
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the user's temporary working
        /// version of the workflow is returned if it exists. If no temporary working version exist, the current
        /// version of the workflow is returned.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">The workflow object with the specified identifier.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier and version.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Workflow>> GetWorkflow(
            [FromRoute] string id,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            WorkflowItem workflowItem = await LoadWorkflowAsync(new UserInformation(User).Id, id, nr, _persistence);
            if (workflowItem is null) return NotFound();
            else return Ok(workflowItem.Workflow);
        }

        /// <summary>
        /// Returns a list stucture containing version information of all available versions for the workflow with
        /// the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">A list structure containing version information is returned. The list is ordered
        /// by decreasing order of version number, except for the first element which will always contain
        /// the current version.
        /// </returns>
        ///  <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier and version.
        /// </returns>
        [Authorize]
        [HttpGet("{id}/versions")]
        public async Task<ActionResult<List<VersionInformation>>> GetWorkflowVersions(
            [FromRoute] string id
        )
        {
            // Retrieve version information (in decreasing order)
            List<VersionInformation> list = await LoadWorkflowVersionsAsync(id);

            // Get the current version number for the user
            int nr = await GetCurrentWorkflowVersionNumber(new UserInformation(User).Id, id, _persistence);
            if (nr == 0) return NotFound();

            // Put the current version entry at the top of the list
            foreach (VersionInformation version in list)
            {
                if (version.Number == nr)
                {
                    list.Remove(version);
                    list.Insert(0, version);
                    break;
                } 
            }

            return list;
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
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Workflow>> PostWorkflow(
            [FromBody] Workflow workflow
        )
        {
            // Create the workflow item
            UserInformation userInformation = new UserInformation(User);
            WorkflowItem workflowItem = new WorkflowItem
            (
                true,
                userInformation.Id,
                workflow,
                new VersionInformation(0, null, userInformation)
            );

            // Write the item
            bool isCreated = await _persistence.WriteDbDocumentAsync(workflowItem.CreateDbDocument());
            if (isCreated)
            {
                return Ok(workflow);
            }
            else
            {
                _logger.LogWarning($"User {userInformation.Name} could not post workflow {workflow.Name}"); 
                return Conflict();
            }
        }

        /// <summary>
        /// Saves the temporary working version of a workflow as a new version.
        /// </summary>
        /// <param name="id">The workflow identifier.</param>
        /// <param name="description">A text description of the version changes.</param>
        /// <request name="Example">
        ///     <body>"Changed selection criteria of graph"</body>
        /// </request>
        /// <returns status="200">
        /// Version information of the newly created version will be attached to the returned object.
        /// </returns>
        /// <returns status="404">
        /// Occurs if the workflow with the specified identifier has no temporary working version.
        /// </returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow version.
        /// </returns>
        [Authorize]
        [HttpPost("{id}/versions")]
        public async Task<ActionResult<VersionInformation>> PostWorkflowVersion(
            [FromRoute] string id,
            [FromBody] string description
        )
        {
            // Retrieve the temporary working version of a workflow
            UserInformation userInformation = new UserInformation(User);
            string userId = userInformation.Id;
            WorkflowItem workflowItem = await LoadTemporaryWorkflowAsync(userId, id, _persistence);
            if (workflowItem is null)
            {
                _logger.LogWarning($"Temporary workflow {id} could not be found for creating a new version");
                return NotFound();
            }

            // Get the maximum version number and update version information
            int maxVersionNumber = 0;
            List<VersionInformation> versionInformationList = await LoadWorkflowVersionsAsync(id);
            if (versionInformationList.Count > 0) maxVersionNumber = versionInformationList[0].Number;
            workflowItem.VersionInformation.BaseNumber = workflowItem.VersionInformation.Number;
            workflowItem.VersionInformation.Number = ++maxVersionNumber;
            workflowItem.VersionInformation.Description = description;
            workflowItem.VersionInformation.CreatedBy = userInformation;
            workflowItem.VersionInformation.DateCreated = DateTime.Now;

            // Perform the database write operations
            WorkflowItem saveWorkflowItem = new WorkflowItem
            (
                false,
                id,
                workflowItem.Workflow,
                workflowItem.VersionInformation
            );
            WorkflowAccessItem saveWorkflowAccessItem = new WorkflowAccessItem
            (
                true,
                userId,
                id,
                workflowItem.Workflow.Name,
                workflowItem.VersionInformation
            );
            WorkflowItem deleteTempWorkflowItem = new WorkflowItem(userId, id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                saveWorkflowItem.SaveDbDocument(),
                saveWorkflowAccessItem.SaveDbDocument(),
                deleteTempWorkflowItem.DeleteDbDocument()
            });
            
            // Return the version information
            if (isSaved)
            {
                return Ok(workflowItem.VersionInformation);
            }
            else
            {
                _logger.LogWarning($"User {userInformation.Name} could not post a new version of workflow " +
                    $"{workflowItem.Workflow.Name}");
                return Conflict();
            }
        }

        /// <summary>
        /// Reverts a workflow to a previous version.
        /// </summary>
        /// <param name="id">The workflow identifier.</param>
        /// <param name="nr">The version number the workflow should be reverted to.</param>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">
        /// Returns version information of the version the workflow was reverted to.
        /// </returns>
        /// <returns status="403">
        /// Occurs if the version number is not a positive integer
        /// </returns>
        /// <returns status="404">
        /// Occurs if the workflow with the specified identifier and version number does not exist. 
        /// </returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow version.
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/versions")]
        public async Task<ActionResult<VersionInformation>> PatchWorkflowVersion(
            [FromRoute] string id,
            [FromQuery(Name = "workflowVersion")] int nr
        )
        {
            // Check that the version number is a valid version
            if (nr <= 0) return BadRequest();

            // Get the workflow and version information of the specified version number
            WorkflowItem workflowItem = await LoadWorkflowAsync(id, nr, _persistence);
            if (workflowItem is null) return NotFound();

            // Perform the database write operations
            string userId = new UserInformation(User).Id;
            WorkflowAccessItem saveWorkflowAccessItem = new WorkflowAccessItem
            (
                true,
                userId,
                id,
                workflowItem.Workflow.Name,
                workflowItem.VersionInformation
            );
            WorkflowItem deleteWorkflowItem = new WorkflowItem(userId, id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                saveWorkflowAccessItem.SaveDbDocument(),
                deleteWorkflowItem.DeleteDbDocument()
            });

            // Return the version information
            if (isSaved)
            {
                return Ok(workflowItem.VersionInformation);
            }
            else
            {
                _logger.LogWarning($"User {new UserInformation(User).Name} could not patch version {nr} of workflow " +
                    $"{workflowItem.Workflow.Name}");
                return Conflict();
            }
        }

        /// <summary>
        /// Deletes an existing workflow object specified by an identifier from the user's view 
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkflow(
            [FromRoute] string id
        )
        {
            string userId = new UserInformation(User).Id;

            // First try and delete the workflow access item
            WorkflowAccessItem deleteWorkflowAccessItem = new WorkflowAccessItem(true, userId, id);
            bool deleted = await _persistence.WriteDbDocumentAsync(deleteWorkflowAccessItem.DeleteDbDocument());

            // If the workflow access item does not exist, this is a temporary workflow to delete
            if (!deleted)
            {
                WorkflowItem deleteWorkflowItem = new WorkflowItem(userId, id);
                deleted = await _persistence.WriteDbDocumentAsync(deleteWorkflowItem.DeleteDbDocument());
            }

            if (deleted) return Ok(); else return NotFound();
        }

        /// <summary>
        /// Gets the list of operations from a workflow object specified by an identifier and optional version number.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the operations on the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the operations on the current version of the workflow is returned.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">The list of operations attached to the workflow.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [Authorize]
        [HttpGet("{id}/operations")]
        public async Task<ActionResult<List<WorkflowOperation>>> GetWorkflowOperations(
            [FromRoute] string id,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            WorkflowItem workflowItem = await LoadWorkflowAsync(new UserInformation(User).Id, id, nr, _persistence);
            if (workflowItem is null) return NotFound();
            else return Ok(workflowItem.Workflow.Operations);
        }

        /// <summary>
        /// Gets an operation specified by index from a workflow object specified by an identifier and optional version
        /// number.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index of the workflow operation.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the operation on the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the operation on the current version of the workflow is returned.</param>
        /// <request name="First">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">0</arg>
        /// </request>
        /// <request name="Last">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">-1</arg>
        /// </request>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">-1</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">The specified operation attached to the workflow.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier and version or there is no
        /// workflow operation corresponding to the specified index.
        /// </returns>
        [Authorize]
        [HttpGet("{id}/operations/{index}")]
        public async Task<ActionResult<WorkflowOperation>> GetWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int index,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            WorkflowItem workflowItem = await LoadWorkflowAsync(new UserInformation(User).Id, id, nr, _persistence);
            if (workflowItem is null) return NotFound();

            // Convert the index and get the operation.
            if (workflowItem.Workflow.Operations is null ||
                workflowItem.Workflow.Operations.Count == 0
            ) return NotFound();
            if (index < 0) index += workflowItem.Workflow.Operations.Count;
            if (index < 0 || index >= workflowItem.Workflow.Operations.Count) return NotFound();
            return Ok(workflowItem.Workflow.Operations[index]);
        }

        /// <summary>
        /// Inserts a new workflow operation at the optionally specified index into the list of operations of a workflow
        /// object specified by an identifier and optional version number. A new temporary working version
        /// of the workflow is created for the user.
        /// </summary>
        /// <param name="id">The identifier of the workflow</param>
        /// <param name="index">The index to insert the workflow operation at. If not specified, the
        /// operation appends the new operation at the end of the list of operations.</param>
        /// <param name="operation">The operation to insert. If not specified, they will be filled with defaults.
        /// </param>
        /// <param name="nr">The version number of the workflow. If not specified, the operation is inserted on the
        /// user's temporary working version of the workflow if it exists. If no temporary working version
        /// exist, the operation is inserted on the current version of the workflow.</param>
        /// <request name="Append">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
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
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
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
        /// <request name="Append with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        ///     <body>
        ///     {
        ///         "action":    
        ///         {
        ///             "type": "decrement"
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
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPost("{id}/operations/{index?}")]
        public async Task<ActionResult<WorkflowOperation>> InsertWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int? index,
            [FromBody] WorkflowOperation operation,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            string userId = new UserInformation(User).Id;
            WorkflowItem workflowItem = await LoadWorkflowAsync(userId, id, nr, _persistence);
            if (workflowItem is null) return NotFound();

            // Try to perform the insertion.
            if (workflowItem.Workflow.Operations is null)
                workflowItem.Workflow.Operations = new List<WorkflowOperation>();
            if (index.HasValue)
            {
                if (index < 0) index += workflowItem.Workflow.Operations.Count;
                if (index < 0 || index > workflowItem.Workflow.Operations.Count) return BadRequest();
                workflowItem.Workflow.Operations.Insert(index.Value, operation);
            }
            else workflowItem.Workflow.Operations.Add(operation);
                
            // Save the changes to the workflow.
            bool updated = await SaveTemporaryWorkflowAsync(userId, id, workflowItem);
            if (updated)
            {
                return Ok(operation);
            }
            else
            {
                _logger.LogWarning($"Operation for workflow {workflowItem.Workflow.Name} could not be inserted");
                return Conflict();
            }
        }

        /// <summary>
        /// Updates an existing workflow operation specified by an index in the list of operations of a workflow
        /// object specified by an identifier and optional version number. A new temporary working version
        /// of the workflow is created for the user. Any properties that are specified in the update will overwrite
        /// existing properties. If a property is not specified, it is not updated in the original object.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index to update the workflow operation at.</param>
        /// <param name="operation">The operation to use to update the existing operation.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the user's temporary working
        /// version of the workflow is updated. If no temporary working version exist, the current version of the
        /// workflow is updated.</param>
        /// <request name="Update First with Name">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">0</arg>
        ///     <body>
        ///     {
        ///         "name": "Updated Operation Name"
        ///     }
        ///     </body>
        /// </request>
        /// <request name="Update Last with Selector">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">-1</arg>
        ///     <body>
        ///     {
        ///         "selector": { "type": "none" }
        ///     }
        ///     </body>
        /// </request>
        /// <request name="Update with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">0</arg>
        ///     <arg name="nr">3</arg>
        ///     <body>
        ///     {
        ///         "name": "Updated Operation Name"
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
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/operations/{index}")]
        public async Task<ActionResult<WorkflowOperation>> PatchWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int index,
            [FromBody] WorkflowOperation operation,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            string userId = new UserInformation(User).Id;
            WorkflowItem workflowItem = await LoadWorkflowAsync(userId, id, nr, _persistence);
            if (workflowItem is null) return NotFound();

            // Try to perform the update.
            if (workflowItem.Workflow.Operations is null) return BadRequest();
            if (index < 0) index += workflowItem.Workflow.Operations.Count;
            if (index < 0 || index >= workflowItem.Workflow.Operations.Count) return BadRequest();

            workflowItem.Workflow.Operations[index].Name =
                operation.Name ?? workflowItem.Workflow.Operations[index].Name;
            workflowItem.Workflow.Operations[index].Selector =
                operation.Selector ?? workflowItem.Workflow.Operations[index].Selector;
            workflowItem.Workflow.Operations[index].Actor =
                operation.Actor ?? workflowItem.Workflow.Operations[index].Actor;

            // Save the changes to the workflow.
            bool updated = await SaveTemporaryWorkflowAsync(userId, id, workflowItem);
            if (updated)
            {
                return Ok(workflowItem.Workflow.Operations[index]);
            }
            else
            {
                _logger.LogWarning($"Operation for workflow {workflowItem.Workflow.Name} could not be patched");
                return Conflict();
            }            
        }

        /// <summary>
        /// Removes an operation optionally specified by index from a workflow object specified by an identifier and
        /// optional version number. If the index of the operation is not specified, the operation removes the last
        /// operation in the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index of the workflow operation.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the operation is deleted from the
        /// user's temporary working version of the workflow is returned if it exists. If no temporary working version
        /// exist, the operation is removed from the current version of the workflow.</param>
        /// <request name="Remove Last">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Remove Second">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">1</arg>
        /// </request>
        /// <request name="Remove with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier or there is no workflow operation
        /// corresponding to the specified index.
        /// </returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpDelete("{id}/operations/{index?}")]
        public async Task<ActionResult> RemoveWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int? index,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            string userId = new UserInformation(User).Id;
            WorkflowItem workflowItem = await LoadWorkflowAsync(userId, id, nr, _persistence);
            if (workflowItem is null) return NotFound();

            // Try to perform the deletion.
            if (workflowItem.Workflow.Operations is null) return BadRequest();
            if (index.HasValue)
            {
                if (index < 0) index += workflowItem.Workflow.Operations.Count;
                if (index < 0 || index >= workflowItem.Workflow.Operations.Count) return BadRequest();
                workflowItem.Workflow.Operations.RemoveAt(index.Value);
            }
            else if (workflowItem.Workflow.Operations.Count > 0)
                workflowItem.Workflow.Operations.RemoveAt(workflowItem.Workflow.Operations.Count - 1);

            // Save the changes to the workflow.
            bool updated = await SaveTemporaryWorkflowAsync(userId, id, workflowItem);
            if (updated)
            {
                return Ok();
            }
            else
            {
                _logger.LogWarning($"Operation for workflow {workflowItem.Workflow.Name} could not be deleted");
                return Conflict();
            }
        }
    }
}
