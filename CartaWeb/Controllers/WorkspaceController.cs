using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


using CartaCore.Serialization.Json;
using CartaCore.Persistence;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Manages creation, retrieval, and archiving of workspaces by the signed in user, and sharing of workspaces
    /// with other users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspaceController : ControllerBase
    {
        /// <summary>
        /// Options for serialization
        /// </summary>
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Static constructor for initializing JSON serialization/deserialization options
        /// </summary>
        static WorkspaceController()
        {
            JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.Converters.Insert(0, new JsonDiscriminantConverter());
        }

        /// <summary>
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<WorkspaceController> _logger;

        /// <summary>
        /// The persistence object used to read and write from the database
        /// </summary>
        private readonly Persistence _persistence;

        /// <inheritdoc />
        public WorkspaceController(ILogger<WorkspaceController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        /// <summary>
        /// Returns the workspace information for the given workspace identifier and user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A workspace item.
        /// </returns>
        protected async Task<WorkspaceItem> LoadWorkspaceItemAsync(string userId, string workspaceId)
        {
            WorkspaceItem workspaceItem = new WorkspaceItem(userId, workspaceId);
            Item item = await _persistence.LoadItemAsync(workspaceItem);
            return (WorkspaceItem)item;
        }

        /// <summary>
        /// Returns user information for the given workspace and user.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The user item.
        /// </returns>
        protected async Task<UserItem> LoadUserItemAsync(string workspaceId, string userId)
        {
            UserItem userItem = new UserItem(workspaceId, userId);
            Item item = await _persistence.LoadItemAsync(userItem);
            return (UserItem)item;
        }

        /// <summary>
        /// Loads data set information for the given workspace identifier and data set identifier
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="datasetId">The dataset identifier</param>
        protected async Task<DatasetItem> LoadWorkspaceDatasetAsync(
            string workspaceId,
            string datasetId
        )
        {
            DatasetItem datasetItem = new DatasetItem(workspaceId, datasetId);
            Item item = await _persistence.LoadItemAsync(datasetItem);
            return (DatasetItem)item;
        }

        /// <summary>
        /// Retrieves accees information on the specified workflow.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns>Workflow access information.</returns>
        protected async Task<WorkflowAccessItem> LoadWorkflowAccessAsync(
            string id,
            string workflowId
        )
        {
            WorkflowAccessItem workflowAccessItem = new WorkflowAccessItem(false, id, workflowId);
            Item item = await _persistence.LoadItemAsync(workflowAccessItem);
            return (WorkflowAccessItem)item;
        }

       
        /// <summary>
        /// Checks whether a specified workspace change date falls within from date and to date criteria.
        /// </summary>
        /// <param name="changeDate">The date and time of the workspace change.</param>
        /// <param name="dateFrom">From which date onwards should a change have occured in.</param>
        /// <param name="dateTo">Prior to which date should a change have occurred in.</param>
        /// <returns>True if the change date falls within the specified range, else false</returns>
        protected static bool IsDateInRange(DateTime changeDate, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if ((dateFrom is not null) & (changeDate < dateFrom)) return false;
            if ((dateTo is not null) & (changeDate > dateTo)) return false;
            return true;
        }

        /// <summary>
        /// Create a new workspace for the given user.
        /// </summary>
        /// <param name="name">The workspace name.</param>
        /// <request name="Example">
        ///     <arg name="name">MyWorkspace</arg>
        /// </request>
        /// <returns status="200">The workspace information will be attached to the returned object.</returns>
        /// <returns status="409">
        /// Occurs when the create operation fails unexpectedly due to a key conflict.
        /// </returns>
        [Authorize]
        [HttpPost("{name}")]
        public async Task<ActionResult<WorkspaceItem>> PostWorkspace(
            [FromRoute] string name
        )
        {
            // Create workspace item 
            UserInformation userInformation = new UserInformation(User);
            WorkspaceItem workspaceItem = new WorkspaceItem(userInformation.Id, name, userInformation);
            DbDocument workspaceItemDbDocument = workspaceItem.CreateDbDocument();

            // Create user item 
            UserItem userItem = new UserItem(workspaceItem.Id, userInformation);
            userItem.DocumentHistory.AddedBy = userInformation;

            // Create workspace change item 
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                workspaceItem.Id,
                userItem.UserInformation.Name,
                WorkspaceActionEnumeration.Added,
                workspaceItem
            );

            // Persist items
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workspaceItemDbDocument,
                userItem.SaveDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(workspaceItem);
            }
            else
            {
                _logger.LogWarning($"Workspace {name} created by user {userInformation.Name} could not be saved");
                return Conflict();
            }        
        }

        /// <summary>
        /// Get the workspace information for the given workspace identifier.
        /// </summary>
        /// <param name="id">A unique workspace identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <returns status="200"> The workspace information will be attached to the returned object.
        /// </returns>
        /// <returns status="404">
        /// Occurs when the workspace item for the given ID cannot be found.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkspaceItem>> GetWorkspace(
            [FromRoute] string id
        )
        {
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(new UserInformation(User).Id, id);
            if (workspaceItem is null) return NotFound();
            else return Ok(workspaceItem);
        }

        /// <summary>
        /// Get the information of all the workspaces the user has access to.
        /// </summary>
        /// <param name="archived">A flag indicating whether archived workspaces should be returned.
        /// Set to true if archived workspaces should be returned, otherwise set to false.
        /// Defaults to false.</param>
        /// <request name="Retrieve Archived Workspaces">
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <request name="Retrieve Active Workspaces">
        ///     <arg name="archived">false</arg>
        /// </request>
        /// <returns status="200">
        /// A list of workspace items will be attached to the returned object.
        /// </returns>
        /// <returns status="404">
        /// Occurs when no workspaces exist for the user.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<WorkspaceItem>>> GetWorkspaces(
            [FromQuery(Name = "archived")] bool? archived
        )
        {
            // If no archived query parameter has been passed, set it to false
            if (!archived.HasValue) archived = false;

            // Load the items and filter items according to archived flag
            WorkspaceItem workspaceItem = new WorkspaceItem(new UserInformation(User).Id);
            IEnumerable<Item> allWorkspaceItems = await _persistence.LoadItemsAsync(workspaceItem);
            if (allWorkspaceItems is null) return NotFound();
            else
            {
                List<WorkspaceItem> workspaceItems = new List<WorkspaceItem>();
                foreach (WorkspaceItem item in allWorkspaceItems)
                {
                    if (item.Archived == archived.Value) workspaceItems.Add(item);
                }
                return Ok(workspaceItems);
            }
        }

        /// <summary>
        /// Get a list of all the users that have access to a workspace
        /// </summary>
        /// <param name="id">The workspace identifier</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <returns status="200">
        /// A list of users will be attached to the returned object. 
        /// </returns>
        /// <returns status="404">
        /// Occurs when no users can be found. 
        /// </returns>
        [Authorize]
        [HttpGet("{id}/users")]
        public async Task<ActionResult<List<UserItem>>> GetWorkspaceUsers(
            [FromRoute] string id
        )
        {
            UserItem userItem = new UserItem(id);
            IEnumerable<Item> readUserItems = await _persistence.LoadItemsAsync(userItem);
            List<UserItem> userItems = new List<UserItem>() { };
            if (readUserItems is null) return NotFound();
            else
            {
                foreach (UserItem item in readUserItems) userItems.Add(item);
                return Ok(userItems);
            }          
        }

        /// <summary>
        /// Update the archived status of a workspace.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="archived">Flag indicating whether the workspace should be archived (true)
        /// or not (false).</param>
        /// <request name="Archive a Workspace">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <request name="Unarchive a Workspace">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="archived">false</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="404">
        /// Occurs when a workspace for the given identifier cannot be found. 
        /// </returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workspace change.
        /// </returns>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult<WorkspaceItem>> PatchWorkspace(
            [FromRoute] string id,
            [FromQuery(Name = "archived")] bool archived
        )
        {
            UserInformation userInformation = new UserInformation(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();
            UserItem userItem = await LoadUserItemAsync(id, userInformation.Id);
            if (userItem is null) return NotFound();
            workspaceItem.Archived = archived;
            WorkspaceChangeItem workspaceChangeItem = null;

            if (archived)
            {
                userItem.DocumentHistory.DateDeleted = DateTime.Now;
                userItem.DocumentHistory.DeletedBy = userInformation;
                workspaceItem.DocumentHistory.DateArchived = DateTime.Now;
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Removed,
                    userItem                    
                );
            }
            else
            {
                userItem.DocumentHistory.DateAdded = DateTime.Now;
                userItem.DocumentHistory.AddedBy = userInformation;
                workspaceItem.DocumentHistory.DateUnarchived = DateTime.Now;
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Added,
                    userItem
                );
            }

            workspaceItem.SetPartitionKeyId(userInformation.Id);
            userItem.SetPartitionKeyId(id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workspaceItem.UpdateDbDocument(),
                userItem.UpdateDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(workspaceItem);
            }
            else
            {
                _logger.LogWarning($"Workspace {workspaceItem.Name} could not be archived/unarchived");
                return Conflict();
            }
        }

        /// <summary>
        /// Add users to the workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="userItems">A list of users (identifier and user name) that should be added to the
        /// workspace.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <body>
        ///         [
        ///            {
        ///               "userInformation": {   
        ///                    "id":"userId1",
        ///                    "name":"user1"
        ///                }
        ///            },
        ///            {
        ///                "userInformation": {   
        ///                    "id":"userId2",
        ///                    "name":"user2"
        ///                }
        ///            }
        ///         ]
        ///     </body>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        [Authorize]
        [HttpPatch("{id}/users")]
        public async Task<ActionResult<List<UserItem>>> PatchWorkspaceUsers(
            [FromRoute] string id,
            [FromBody] List<UserItem> userItems
        )
        {
            UserInformation userInformation = new UserInformation(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();

            List<UserItem> writeUserItems = new List<UserItem>() { };
            foreach (UserItem userItem in userItems)
            {
                workspaceItem.SetPartitionKeyId(userItem.UserInformation.Id);

                UserItem writeUserItem = new UserItem(id, userItem.UserInformation);
                writeUserItem.DocumentHistory.DateAdded = DateTime.Now;
                writeUserItem.DocumentHistory.AddedBy = userInformation;
                writeUserItems.Add(writeUserItem);

                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    userInformation.Name,
                    WorkspaceActionEnumeration.Added,
                    writeUserItem
                );
             
                bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
                {
                    workspaceItem.SaveDbDocument(),
                    writeUserItem.SaveDbDocument(),
                    workspaceChangeItem.CreateDbDocument()
                });
                if (!isSaved)
                {
                    _logger.LogWarning($"User {userInformation.Name} could not be added to workspace " +
                        $"{workspaceItem.Name}");
                    return Conflict();
                }
            }

            return Ok(writeUserItems);
        }

        /// <summary>
        /// Deletes a user from a workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="users">A list of user identifiers.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="users">846817bf-5695-4cca-bd3b-64313beecb56</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="403">Occurs when the workspace is not owned by the logged in user.</returns>
        /// <returns status="404">Occurs when the user or workspace record could not be found.</returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpDelete("{id}/users")]
        public async Task<ActionResult> DeleteWorkspaceUser(
            [FromRoute] string id,
            [FromQuery(Name = "users")] List<string> users
        )
        {
            foreach (string userId in users)
            {
                // Load the workspace and user items
                WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, id);
                if (workspaceItem is null) return NotFound();
                UserItem userItem = await LoadUserItemAsync(id, userId);
                if (userItem is null) return NotFound();

                // Delete the workspace for the given user if the workspace is owned by the logged in user
                UserInformation currentUser = new UserInformation(User);
                if (workspaceItem.DocumentHistory.AddedBy.Id != currentUser.Id)
                {
                    _logger.LogWarning($"Workspace item for user {userId} and workspace {workspaceItem.Id} " +
                        $"not created by logged in user");
                    return Forbid();
                }

                // Create structure to record the change
                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    currentUser.Name,
                    WorkspaceActionEnumeration.Removed,
                    userItem
                );

                // Perform the database operations
                workspaceItem.SetPartitionKeyId(userId);
                userItem.SetPartitionKeyId(id);
                bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
                {
                    workspaceItem.DeleteDbDocument(),
                    userItem.DeleteDbDocument(),
                    workspaceChangeItem.CreateDbDocument()
                });
                if (!isSaved)
                {
                    _logger.LogWarning($"User {userItem.UserInformation.Name} could not be deleted from workspace " +
                        $"{workspaceItem.Name}");
                    return Conflict();
                }
            }
            // Return Ok if no errors occurred up to this point
            return Ok();
        }

        /// <summary>
        /// Persist the data set under the given workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="source">The data set source.</param>
        /// <param name="resource">The data set resource.</param>
        /// <param name="name">Optional name for the data set.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="source">Synthetic</arg>
        ///     <arg name="resource">Finite</arg>
        /// </request>
        /// <request name="Example with name">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="source">Synthetic</arg>
        ///     <arg name="resource">Finite</arg>
        ///     <arg name="name">MyDatasetName</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The data set information,
        /// including an unique identifier, will be attached to the response.</returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPost("{id}/data/{source}/{resource}")]
        public async Task<ActionResult<DatasetItem>> PostWorkspaceData(
            [FromRoute] string id,
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery(Name = "name")] string name
        )
        {
            UserInformation userInformation = new UserInformation(User);
            DatasetItem datasetItem = new DatasetItem(id, source, resource, userInformation);
            if (name is not null) datasetItem.Name = name;
            
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                id,
                userInformation.Name,
                WorkspaceActionEnumeration.Added,
                datasetItem
            );
            workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
            workspaceChangeItem.WorkspaceChangeInformation.DatasetSource = source.ToString();
            workspaceChangeItem.WorkspaceChangeInformation.DatasetResource = resource;
            
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                datasetItem.CreateDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(datasetItem);
            }
            else
            {
                _logger.LogWarning($"Dataset {source}/{resource} could not be saved under workflow with ID {id}");
                return Conflict();
            }
        }

        /// <summary>
        /// Update the name or workflow of a dataset.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="datasetId">The unique data set identifier.</param>
        /// <param name="name">Optional name for the data set.</param>
        /// <param name="workflowId">Optional workflow identifier to apply to the data set.</param>
        /// <param name="versionNumber">Optional workflow version number to apply to the data set.</param>
        /// <request name="Example to update name">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        ///     <arg name="name">MyDataset</arg>
        /// </request>
        /// <request name="Example to update workflow">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        ///     <arg name="workflow">01F6Q75NC7TEPXWSNSXJC18BDE</arg>
        /// </request>
        /// <request name="Example to update name, workflow and workflow version">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        ///     <arg name="name">MyDatasetName</arg>
        ///     <arg name="workflowId">01F6Q75NC7TEPXWSNSXJC18BDE</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The
        /// data set information will be attached to the response.</returns>
        /// <returns status="400">Occurs when a version number is specified but a workflow identifier is not</returns>
        /// <returns status="404">Occurs when the data set or workflow could not be found.</returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/data/{datasetId}")]
        public async Task<ActionResult<DatasetItem>> PatchWorkspaceData(
            [FromRoute] string id,
            [FromRoute] string datasetId,
            [FromQuery(Name = "name")] string name,
            [FromQuery(Name = "workflow")] string workflowId,
            [FromQuery(Name = "workflowVersion")] int? versionNumber
        )
        {
            if (versionNumber.HasValue & (workflowId is null)) return BadRequest();

            UserInformation userInformation = new UserInformation(User);
            DatasetItem datasetItem = await LoadWorkspaceDatasetAsync(id, datasetId);
            if (datasetItem is null) return NotFound();
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                id,
                userInformation.Name,
                WorkspaceActionEnumeration.Updated,
                datasetItem
            );
            workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
            if (name is not null)
            {
                datasetItem.Name = name;
                workspaceChangeItem.WorkspaceChangeInformation.DatasetSource = datasetItem.Source.ToString();
                workspaceChangeItem.WorkspaceChangeInformation.DatasetResource = datasetItem.Resource;
            }
            if (workflowId is not null)
            {
                WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
                if (workflowAccessItem is null) return NotFound();
                datasetItem.WorkflowId = workflowId;
                workspaceChangeItem.WorkspaceChangeInformation.WorkflowId = workflowId;
                workspaceChangeItem.WorkspaceChangeInformation.WorkflowName = workflowAccessItem.Name;
                if (versionNumber.HasValue)
                {
                    datasetItem.VersionNumber = versionNumber.Value;
                    workspaceChangeItem.WorkspaceChangeInformation.WorkflowVersion = versionNumber.Value;
                }           
            }

            datasetItem.SetPartitionKeyId(id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                datasetItem.UpdateDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(datasetItem);
            }
            else
            {
                _logger.LogWarning($"Dataset with ID {datasetId} could not be patched under workspace with ID {id}");
                return Conflict();
            }   
        }

        /// <summary>
        /// Retrieves all the data sets under the given workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <request name="Example">
        ///     <arg name="workspaceId">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.
        /// A list of data sets will be attached to the returned object.</returns>
        [Authorize]
        [HttpGet("{id}/data")]
        public async Task<ActionResult<List<DatasetItem>>> GetWorkspaceDatasets(
            [FromRoute] string id
        )
        {
            DatasetItem datasetItem = new DatasetItem(id);
            IEnumerable<Item> readDatasetItems = await _persistence.LoadItemsAsync(datasetItem);
            List<DatasetItem> datasetItems = new List<DatasetItem>() { };
            foreach (DatasetItem item in readDatasetItems) datasetItems.Add(item);
            return Ok(datasetItems);
        }

        /// <summary>
        /// Retrieves data set information for the specified workspace and data set.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="datasetId">The data set identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.
        /// The data set information will be attached to the returned object.</returns>
        /// <returns status="404">Occurs when the data set could not be found.</returns>
        [Authorize]
        [HttpGet("{id}/data/{datasetId}")]
        public async Task<ActionResult<DatasetItem>> GetWorkspaceDataset(
            [FromRoute] string id,
            [FromRoute] string datasetId
        )
        {
            DatasetItem datasetItem = await LoadWorkspaceDatasetAsync(id, datasetId);
            if (datasetItem is null) return NotFound();
            else return Ok(datasetItem);
        }

        /// <summary>
        /// Delete the data set from the given workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="datasetId">The data set identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="404">Occurs when the data set could not be found.</returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpDelete("{id}/data/{datasetId}")]
        public async Task<ActionResult> DeleteWorkspaceData(
            [FromRoute] string id,
            [FromRoute] string datasetId
        )
        {
            DatasetItem datasetItem = await LoadWorkspaceDatasetAsync(id, datasetId);
            if (datasetItem is null) return NotFound();
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                id,
                new UserInformation(User).Name,
                WorkspaceActionEnumeration.Removed,
                datasetItem
            );
            workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
            workspaceChangeItem.WorkspaceChangeInformation.DatasetSource = datasetItem.Source.ToString();
            workspaceChangeItem.WorkspaceChangeInformation.DatasetResource = datasetItem.Resource;

            datasetItem.SetPartitionKeyId(id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                datasetItem.DeleteDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok();
            }
            else
            {
                _logger.LogWarning($"Dataset with ID {id} could not be deleted under workflow with ID {id}");
                return Conflict();
            }    
        }

        /// <summary>
        /// Persists a workflow under the given workspace
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="versionNumber">Optional workflow version number.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Example with version number">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow information
        /// will be attached to the response object.</returns>
        /// <returns status="404">Occurs when a workflow with the specified identifier and version
        /// could not be found.</returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPost("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> PostWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId,
            [FromQuery(Name = "workflowVersion")] int? versionNumber 
        )
        {
            // Get user information
            UserInformation userInformation = new UserInformation(User);
            string userId = userInformation.Id;

            // Set the version number
            int nr;
            if (versionNumber.HasValue) nr = versionNumber.Value;
            else nr = await WorkflowController.GetCurrentWorkflowVersionNumber(userId, workflowId, _persistence);
                
            // Load workflow information
            WorkflowItem workflowItem = await WorkflowController.LoadWorkflowAsync(workflowId, nr, _persistence);
            if (workflowItem is null) return NotFound();

            // Persist access information and record change history
            WorkflowAccessItem workflowAccessItem = new WorkflowAccessItem
            (
                false,
                id,
                workflowId,
                workflowItem.Workflow.Name,
                workflowItem.VersionInformation
            );
            workflowAccessItem.DocumentHistory = new DocumentHistory(new UserInformation(User));
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                id,
                userInformation.Name,
                WorkspaceActionEnumeration.Added,
                workflowAccessItem
            );
            workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
            workspaceChangeItem.WorkspaceChangeInformation.WorkflowVersion = workflowItem.VersionInformation.Number;

            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workflowAccessItem.SaveDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(workflowAccessItem);
            }
            else
            {
                _logger.LogWarning($"Workflow {workflowItem.Workflow.Name} could not be saved under workspace ID {id}");
                return Conflict();
            }
        }

        /// <summary>
        /// Retrieves information on workflows that are accessible under a workspace.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="archived">A flag indicating whether archived workspaces should be returned.
        /// Set to true if archived workspaces should be returned, otherwise set to false.
        /// Defaults to false.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <request name="Example to retrieve archived workflows">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow information
        /// will be attached to the response object.</returns>
        [Authorize]
        [HttpGet("{id}/workflows")]
        public async Task<ActionResult<List<WorkflowAccessItem>>> GetWorkspaceWorkflows(
            [FromRoute] string id,
            [FromQuery(Name = "archived")] bool? archived
        )
        {
            // If no archived query parameter has been passed, set it to false
            bool isArchived = false;
            if (archived.HasValue) isArchived = archived.Value;

            // Retrieve the workflow access items
            List<WorkflowAccessItem> workflowAccessItems = new() { };
            WorkflowAccessItem readWorkflowAccessItem = new WorkflowAccessItem(false, id);
            IEnumerable<Item> readWorkflowAccesItems = await _persistence.LoadItemsAsync(readWorkflowAccessItem);
            foreach (WorkflowAccessItem workflowAccessItem in readWorkflowAccesItems)
            {
                if (workflowAccessItem.Archived == isArchived) workflowAccessItems.Add(workflowAccessItem);
            }

            // Return the list of items
            return Ok(workflowAccessItems);
        }

        /// <summary>
        /// Retrieves information on the specified workflow.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow information
        /// will be attached to the response object.</returns>
        /// <returns status="404">Occurs when no workflow information could be found.</returns>
        [Authorize]
        [HttpGet("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> GetWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId
        )
        {
            WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
            if (workflowAccessItem is null) return NotFound();
            else return Ok(workflowAccessItem);
        }

        /// <summary>
        /// Archives/unarchives a workflow from the workspace, or updates the version of the workflow available
        /// under the workspace
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="archived">Flag indicating whether the workflow should be archived (true)
        /// or not (false).</param>
        /// <param name="versionNumber">Optional workflow version number that should be accessible under the
        /// workspace.</param>
        /// <request name="Example to archive a workflow">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <request name="Example to update version number">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <request name="Example to unarchive a workflow and set it to a specific version">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="archived">false</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow access information
        /// will be attached to the response object.</returns>
        /// <returns status="404">Occurs when a workflow with the specified identifier 
        /// could not be found.</returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> PatchWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId,
            [FromQuery(Name = "archived")] bool? archived,
            [FromQuery(Name = "workflowVersion")] int? versionNumber
        )
        {
            // Load the workflow
            WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
            if (workflowAccessItem is null)
            {
                _logger.LogWarning($"Workflow access item for workspace id {id} and workflow {workflowId} could " +
                    $"not be found for patching");
                return NotFound();
            }

            // Check that at least one of the optional parameters is set - otherwise no changes will be affected
            if ((!archived.HasValue) & (!versionNumber.HasValue)) return BadRequest();

            // Update archive information
            WorkspaceChangeItem workspaceChangeItem = null;
            if (archived.HasValue)
            {
                workflowAccessItem.Archived = archived.Value;
                if (archived.Value)
                {
                    workflowAccessItem.DocumentHistory.ArchivedBy = new UserInformation(User);
                    workflowAccessItem.DocumentHistory.DateArchived = DateTime.Now;
                    workspaceChangeItem = new WorkspaceChangeItem
                    (
                        id,
                        workflowAccessItem.DocumentHistory.ArchivedBy.Name,
                        WorkspaceActionEnumeration.Removed,
                        workflowAccessItem
                    );
                }
                else
                {
                    workflowAccessItem.DocumentHistory.UnarchivedBy = new UserInformation(User);
                    workflowAccessItem.DocumentHistory.DateUnarchived = DateTime.Now;
                    workspaceChangeItem = new WorkspaceChangeItem
                    (
                        id,
                        workflowAccessItem.DocumentHistory.UnarchivedBy.Name,
                        WorkspaceActionEnumeration.Added,
                        workflowAccessItem
                    );
                }
            }

            // Update version information
            if (versionNumber.HasValue)
            {
                WorkflowItem workflowItem = await WorkflowController.LoadWorkflowAsync
                (
                    workflowId,
                    versionNumber.Value,
                    _persistence
                );
                if (workflowItem is null)
                {
                    _logger.LogWarning($"Workflow item for workspace id {id} and workflow {workflowId} could not be " +
                        $"found for patching");
                    return NotFound();
                }
                workflowAccessItem.VersionInformation = workflowItem.VersionInformation;
                UserInformation userInformation = new UserInformation(User);
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    userInformation.Name,
                    WorkspaceActionEnumeration.Updated,
                    workflowAccessItem
                );
                workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
                workspaceChangeItem.WorkspaceChangeInformation.WorkflowVersion = workflowItem.VersionInformation.Number;
            }

            // Write information
            workflowAccessItem.SetPartitionKeyId(id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workflowAccessItem.SaveDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(workflowAccessItem);
            }
            else
            {
                _logger.LogWarning($"Workflow with ID {workflowId} could not be patched under workspace ID {id}");
                return Conflict();
            }
        }

        /// <summary>
        /// Deletes the specified workflow
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="403">Occurs when the workflow was not originally added by the logged in user.</returns>
        /// <returns status="404">Occurs when no workflow information could be found.</returns>
        [Authorize]
        [HttpDelete("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> DeleteWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId
        )
        {
            WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
            UserInformation currentUser = new UserInformation(User);
            if (workflowAccessItem is null) return NotFound();
            if (workflowAccessItem.DocumentHistory.AddedBy.Id != currentUser.Id) return Forbid();
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                id,
                currentUser.Name,
                WorkspaceActionEnumeration.Removed,
                workflowAccessItem
            );

            workflowAccessItem.SetPartitionKeyId(id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workflowAccessItem.DeleteDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok();
            }
            else
            {
                _logger.LogWarning($"Workflow with IH {workflowId} could not be deleted from workspace ID {id}");
                return Conflict();
            }
        }

        /// <summary>
        /// Retrieve a summary of changes made to a workspace.
        /// </summary>
        /// <param name="id">A unique workspace identifier.</param>
        /// <param name="type">The type of workspace change to return - Workspace, User, Workflow or Dataset.
        /// If not specified, changes of all types will be returned.</param>
        /// <param name="dateFrom">An ISO-8601 formatted string specifying from which date onwards changes should be
        /// returned. If not specified, no from date filter will be applied.</param>
        /// <param name="dateTo">An ISO-8601 formatted string specifying the date prior to whichchanges should be
        /// returned. If not specified, no to date filter will be applied.</param>
        /// <request name="Example all changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <request name="Example all workspace changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">Workspace</arg>
        /// </request>
        /// <request name="Example all user changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">User</arg>
        /// </request>
        /// <request name="Example all workflow changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">Workflow</arg>
        /// </request>
        /// <request name="Example all dataset changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">Dataset</arg>
        /// </request>
        /// <request name="Example all changes from a specified date onwards">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="dateFrom">2021-01-01</arg>
        /// </request>
        /// <request name="Example all changes prior to a specified date">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="dateTo">2021-01-01</arg>
        /// </request>
        /// <request name="Example all changes between a specified date range">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="dateFrom">2021-01-01</arg>
        ///     <arg name="dateTo">2021-01-31</arg>
        /// </request>
        /// <returns status="200">A list of workspace changes will be attached to the returned object.
        /// The list will be empty if no changes corresponding to the specified criteria exist. 
        /// </returns>
        /// <returns status="400">
        /// Occurs when the specified <code>toDate</code> occurs before the specified <code>fromDate</code>
        /// </returns>
        [Authorize]
        [HttpGet("{id}/changes")]
        public async Task<ActionResult<List<WorkspaceChangeItem>>> GetWorkspaceChanges(
            [FromRoute] string id,
            [FromQuery(Name = "type")] WorkspaceChangeEnumeration? type,
            [FromQuery(Name = "dateFrom")] DateTime? dateFrom,
            [FromQuery(Name = "dateTo")] DateTime? dateTo
        )
        {
            // Check that dateTo does not precede dateFrom
            if (dateFrom.HasValue & dateTo.HasValue)
                if (dateFrom.Value > dateTo.Value)
                    return BadRequest();

            // Retrieve all changes in the workspace of the relevant type
            WorkspaceChangeItem readWorkspaceChangeItem = null;
            if (type.HasValue) readWorkspaceChangeItem = new WorkspaceChangeItem(id, type.Value);
            else readWorkspaceChangeItem = new WorkspaceChangeItem(id);
            IEnumerable<Item> readWorkspaceChangeItems = await _persistence.LoadItemsAsync(readWorkspaceChangeItem);

            // Filter changes by date
            List<WorkspaceChangeItem> workspaceChangeItems = new() { };
            foreach (WorkspaceChangeItem workspaceChangeItem in readWorkspaceChangeItems)
            {
                if (IsDateInRange(workspaceChangeItem.WorkspaceAction.DateTime, dateFrom, dateTo))
                    workspaceChangeItems.Add(workspaceChangeItem);
            }
        
            // Sort the list by date
            workspaceChangeItems.Sort((q, p) => p.WorkspaceAction.DateTime.CompareTo(q.WorkspaceAction.DateTime));

            // Return changes
            return Ok(workspaceChangeItems);
        }
        
    }
        
}
