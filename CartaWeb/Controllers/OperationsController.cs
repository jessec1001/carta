using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Operations;
using CartaCore.Persistence;
using CartaCore.Utilities;
using CartaWeb.Models.DocumentItem;
using CartaWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NJsonSchema;
using NJsonSchema.Generation;

namespace CartaWeb.Controllers
{
    // TODO: Make sure that operation defaults are added to the internal representation of operations once created.
    // TODO: Add searching for workflows in search endpoint
    // TODO: Implement destroying results records manually.

    /// <summary>
    /// Represents the side of an operation that a field exists on.
    /// This may either be the input or output side.
    /// </summary>
    public enum OperationSide
    {
        /// <summary>
        /// The input side of the operation.
        /// </summary>
        Input,
        /// <summary>
        /// The output side of the operation.
        /// </summary>
        Output,
    }

    /// <summary>
    /// A controller that manages creating, updating, retrieving, and deleting operations that can be executed with
    /// inputs to produce a set of outputs as a result.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        // TODO: This singleton service should probably be interface-ified and should be given a better name.
        private readonly OperationTaskCollection TaskCollection;
        private readonly ILogger<OperationsController> _logger;
        private readonly Persistence _persistence;

        public OperationsController(ILogger<OperationsController> logger, INoSqlDbContext noSqlDbContext, OperationTaskCollection taskCollection)
        {
            TaskCollection = taskCollection;
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        #region Persistence
        // TODO: Redo file storage to store uploads based on index and per-job.
        public static Stream ReadFile()
        {
            if (!System.IO.File.Exists("file")) return null;
            Stream stream = System.IO.File.Open("file", FileMode.Open);
            return stream;
        }
        public static void SaveFile(Stream stream)
        {
            using (FileStream fileStream = System.IO.File.Open("file", FileMode.Create))
            {
                stream.CopyTo(fileStream);
                stream.Close();
            }
        }

        /// <summary>
        /// Saves the specified operation to the database.
        /// </summary>
        /// <param name="operationItem">The operation item.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> SaveOperationAsync(OperationItem operationItem, Persistence _persistence)
        {
            DbDocument document = operationItem.CreateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        public static async Task<bool> UpdateOperationAsync(OperationItem operationItem, Persistence _persistence)
        {
            DbDocument document = operationItem.UpdateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Deletes the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The identifier of the operation.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> DeleteOperationAsync(string operationId, Persistence _persistence)
        {
            OperationItem operationItem = new(operationId);
            DbDocument document = operationItem.DeleteDbDocument();
            bool isDeleted = await _persistence.WriteDbDocumentAsync(document);
            return isDeleted;
        }
        /// <summary>
        /// Loads the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The identifier of the operation.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>The loaded operation item.</returns>
        public static async Task<OperationItem> LoadOperationAsync(string operationId, Persistence _persistence)
        {
            OperationItem operationItem = new(operationId);
            Item item = await _persistence.LoadItemAsync(operationItem);
            return (OperationItem)item;
        }

        /// <summary>
        /// Saves the specified job to the database.
        /// </summary>
        /// <param name="jobItem">The job item.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> SaveJobAsync(JobItem jobItem, Persistence _persistence)
        {
            DbDocument document = jobItem.CreateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        public static async Task<bool> UpdateJobAsync(JobItem jobItem, Persistence _persistence)
        {
            DbDocument document = jobItem.UpdateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Deletes the specified job from the database.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static Task<bool> DeleteJobAsync(string jobId, string operationId, Persistence _persistence)
        {
            JobItem jobItem = new(jobId, operationId);
            DbDocument document = jobItem.DeleteDbDocument();
            return _persistence.WriteDbDocumentAsync(document);
        }
        /// <summary>
        /// Loads the specified job from the database.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>The loaded job item.</returns>
        public static async Task<JobItem> LoadJobAsync(string jobId, string operationId, Persistence _persistence)
        {
            JobItem jobItem = new(jobId, operationId);
            Item item = await _persistence.LoadItemAsync(jobItem);
            return (JobItem)item;
        }
        /// <summary>
        /// Loads the jobs for the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>The loaded job items.</returns>
        public static async Task<List<JobItem>> LoadJobsAsync(string operationId, Persistence _persistence)
        {
            JobItem jobItem = new(null, operationId);
            IEnumerable<Item> items = await _persistence.LoadItemsAsync(jobItem);
            List<JobItem> jobItems = items.Cast<JobItem>().ToList();
            return jobItems;
        }
        #endregion

        #region Endpoints (Operation Types)
        /// <summary>
        /// Retrieves a list of operation types that are available to the user. If no filters are specified, this list
        /// will contain all operations. If filters are specified, the operation types must match them.
        /// </summary>
        /// <param name="filterName">
        /// An optional name used to filter operations. If specified, only operations containing this name as a
        /// substring of its display name (case insensitive) will be returned.
        /// </param>
        /// <param name="filterTags">
        /// An optional set of tags used to filter operations. If specified, only operations containing at least one of
        /// the specified tags will be returned.
        /// </param>
        /// <param name="limit">
        /// A limit on the number of results that are returned. If not specified, all operations are returned.
        /// </param>
        /// <returns>
        /// A list of (optionally filtered) operation types with their type and display names, and a description.
        /// </returns>
        [HttpGet("types")]
        public async Task<ActionResult<OperationDescription[]>> GetOperationTypes(
            [FromQuery(Name = "name")] string filterName,
            [FromQuery(Name = "tags")] string[] filterTags,
            [FromQuery(Name = "limit")] int? limit)
        {
            // TODO: Reimplement workflow types.
            // Get the descriptions for all of the operations.
            OperationDescription[] descriptionsSimple = OperationDescription.FromOperationTypes();
            OperationDescription[] descriptionsWorkflow = await WorkflowsController.LoadWorkflowDescriptionsAsync(_persistence);
            OperationDescription[] descriptions = new OperationDescription
            [
                descriptionsSimple.Length + descriptionsWorkflow.Length
            ];
            descriptionsSimple.CopyTo(descriptions, 0);
            descriptionsWorkflow.CopyTo(descriptions, descriptionsSimple.Length);

            // If the tags filter was specified, we filter operations based on the union of tags specified.
            // That is, if any of the specified tags exist on the operation description, that operation is included.
            if (filterTags is not null && filterTags.Length > 0)
            {
                // We use lowercase tags in order to make the API more convenient for a manual user.
                filterTags = filterTags
                    .Select(tag => tag?.ToLower() ?? "")
                    .ToArray();
                descriptions = descriptions
                    .Where(description =>
                    {
                        IEnumerable<string> lowercaseTags = description.Tags.Select(tag => tag.ToLower());
                        return lowercaseTags.Intersect(filterTags).Any();
                    })
                    .ToArray();
            }

            // If the name filter was specified, we sort the names based on their string similarity distance.
            if (filterName is not null)
            {
                // We use the lowercase name in order to make the API more convenient for a manual user.
                filterName = filterName.ToLower();
                descriptions = descriptions
                    .OrderByDescending((description) =>
                    {
                        string source = filterName;
                        string target = description.Display.ToLower();

                        double similarity1 = source.SubsequenceSimilarity(target) / target.Length;
                        double similarity2 = target.SubsequenceSimilarity(source) / source.Length;
                        return Math.Max(similarity1, similarity2);
                    })
                    .ToArray();
            }

            // If the limit is specified, limit the number of results to that number of types.
            if (limit.HasValue)
            {
                descriptions = descriptions
                    .Take(limit.Value)
                    .ToArray();
            }

            return Ok(descriptions);
        }
        #endregion
        #region Endpoints (Operation CRUD)
        /// <summary>
        /// Creates a new operation instance. The type of operation must be specified. The operation identifier cannot
        /// be set by the client.
        /// </summary>
        /// <returns status="200">
        /// The created operation as represented on the service after initialization.
        /// </returns>
        /// <returns status="400">
        /// Nothing if a required property was omitted or invalid or a non-mutable property was specified.
        /// </returns>
        public async Task<ActionResult<OperationItem>> CreateOperation(
            [FromBody] OperationItem operationItem
        )
        {
            // TODO: Also incorporate workflow types.
            // Create the operation instance.
            // The defaults for this instance should be automatically generated when constructed.
            Type operationType = Operation.TypeFromName(operationItem.Type);
            OperationDescription description = OperationDescription.FromType(operationType);

            // If we did not find an operation type, we return a bad request error.
            if (operationType is null)
            {
                return BadRequest(new
                {
                    Error = "Operation type could not be found.",
                    Type = operationItem.Type
                });
            }

            // Check if the operation is a workflow.
            WorkflowItem workflowItem = null;
            if (operationType.IsAssignableTo(typeof(WorkflowOperation)))
            {
                workflowItem = await WorkflowsController.LoadWorkflowAsync(operationItem.Subtype, _persistence);
                if (workflowItem is null)
                {
                    return BadRequest(new
                    {
                        Error = "Operation workflow type could not be found.",
                        Subtype = operationItem.Subtype
                    });
                }
            }

            // Set a new identifier.
            // We make sure that the identifier has not already been set by the user otherwise, this is a bad request.
            if (operationItem.Id is not null)
            {
                return BadRequest(new
                {
                    Error = "Operation identifier should not be specified.",
                    Parameter = nameof(OperationItem.Id)
                });
            }
            // Make sure to set a default name if one was not specified.
            if (operationItem.Name is null)
            {
                operationItem.Name = workflowItem is null ? description.Display : workflowItem.Name;
            }

            operationItem.Id = NUlid.Ulid.NewUlid().ToString();
            operationItem.Default ??= new Dictionary<string, object>();

            // Save the created operation item and return its internal representation.
            await SaveOperationAsync(operationItem, _persistence);
            return Ok(operationItem);
        }
        /// <summary>
        /// Updates an operation instance specified by its unique identifier. Specified properties will be overwritten
        /// on the resource. Non-specified properties will be left as-is. Neither the operation identifier nor the
        /// operation type can be modified.   
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="mergingOperationItem">The operation item to merge with the existing one.</param>
        /// <returns status="200">
        /// The updated operation as represented on the service after updating.
        /// </returns>
        /// <returns status="400">
        /// Nothing when the user tries to modify a non-mutable property.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [HttpPatch("{operationId}")]
        public async Task<ActionResult<OperationItem>> UpdateOperation(
            [FromRoute] string operationId,
            [FromBody] OperationItem mergingOperationItem)
        {
            // TODO: Updating an operation may cause connections to become invalid (for instance in dynamic fields).
            // TODO: Updating an operation causes the containing workflow to become cache invalid.

            // Get the existing operation instance.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Check that no reserved fields have been set.
            if (mergingOperationItem.Id is not null && mergingOperationItem.Id != operationItem.Id)
            {
                return BadRequest(new
                {
                    Error = "Operation identifier cannot be modified.",
                    Parameter = nameof(OperationItem.Id)
                });
            }
            if (
                (mergingOperationItem.Type is not null && mergingOperationItem.Type != operationItem.Type) ||
                (mergingOperationItem.Subtype is not null && mergingOperationItem.Subtype != operationItem.Subtype)
            )
            {
                return BadRequest(new
                {
                    Error = "Operation type cannot be modified.",
                    Parameter = nameof(OperationItem.Type)
                });
            }

            // We merge together the existing and merging operation items.
            mergingOperationItem.Id = operationItem.Id;
            mergingOperationItem.Type = operationItem.Type;
            mergingOperationItem.Subtype = operationItem.Subtype;
            mergingOperationItem.Default ??= operationItem.Default;

            // Save the updated operation item and return its internal representation.
            await UpdateOperationAsync(mergingOperationItem, _persistence);
            return Ok(mergingOperationItem);
        }
        /// <summary>
        /// Deletes an operation instance specified by its unique identifier.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <returns status="200">
        /// Nothing.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [HttpDelete("{operationId}")]
        public async Task<ActionResult> DeleteOperation(
            [FromRoute] string operationId)
        {
            // Try to delete the operation item from store.
            bool success = await DeleteOperationAsync(operationId, _persistence);
            // TODO: Delete results associated with the operation.

            // We return not found if the operation item does not exist.
            if (!success)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }
            else return Ok();
        }
        /// <summary>
        /// Gets an operation instance specified by its unique identifier.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <returns status="200">
        /// The operation instance.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [HttpGet("{operationId}")]
        public async Task<ActionResult<OperationItem>> GetOperation(
            [FromRoute] string operationId)
        {
            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            // We return not found if the operation item does not exist.
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }
            else return Ok(operationItem);
        }
        #endregion
        #region Endpoints (Operation Execution)
        /// <summary>
        /// Executes an operation specified by its unique identifier on a particular input. 
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="input">The input to provide to the operation.</param>
        /// <returns status="200">
        /// A unique result identifier that should be used to retrieve the result once available.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [HttpPost("{operationId}/execute")]
        public async Task<ActionResult<JobItem>> ExecuteOperation(
            [FromRoute] string operationId,
            [FromBody] Dictionary<string, object> input)
        {
            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            // We return not found if the operation item does not exist.
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // TODO: Perform type conversion here.
            // TODO: We cannot perform this value conversion on a workflow yet. We need the workflow to expose its
            //       expected types.
            // We read in the body object.
            // We use the operation instance to get input types for each of the fields.
            Operation operation = await InstantiateOperation(operationItem, _persistence);

            // We get the operation instance itself from the item format.
            OperationContext context = new()
            {
                Parent = null,
                Operation = operation,

                Default = operationItem.Default,
                Input = input,
                Output = new Dictionary<string, object>(),

                // TODO: Implement thread limit.
                // ThreadCount = 32
            };

            // TODO: Account for whether the operation is deterministic.
            // TODO: Get the hash of these inputs with respect to the input type (the input type matters because it may
            //       have hashing-related annotations such as [UnsortedArray]).
            // TODO: Modifying any operation inside of a workflow means that cached results should be invalid.
            // TODO: Reimplement caching for operations and when applicable, their suboperations.
            // We check if the result already exists before trying to run the operation.
            // TODO: This caching mechanism should use a sort of hash of the operation to account for changing workflows.
            //       For instance, this hash could be as simple as the unique identifier of the operation for simple
            //       operations but constructed from all of the sub-hashes for workflow operations. To do this, we must
            //       simply implement the hashable interface defined in `Carta.Hashing`.
            string resultId = (await context.Total.ComputeHashAsync()).ToHexString();

            // Construct a job item for the executing operation instance.
            JobItem job = new(resultId, operationId)
            {
                Completed = false,
                Value = context.Total,
                Result = null,
                Tasks = new List<OperationTask>(operation.GetTasks(context)),
            };

            // TODO: Instead of executing the operation, send it to the service that collects a list of executing operations.
            // We wait for the operation to complete and store the results.
            await SaveJobAsync(job, _persistence);
            TaskCollection.Push((serviceScopeFactory) =>
            {
                return (job, operation, context);
            });

            // We return the ID for the result.
            return Ok(job);
        }
        [HttpGet("{operationId}/jobs")]
        public async Task<ActionResult<List<JobItem>>> RetrieveOperationJobs(
            [FromRoute] string operationId
        )
        {
            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the jobs for the operation.
            List<JobItem> jobs = await LoadJobsAsync(operationId, _persistence);
            return jobs;
        }
        [HttpGet("{operationId}/jobs/{jobId}/{field?}/{selector?}")]
        public async Task<ActionResult<JobItem>> RetrieveOperationJob(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string field,
            [FromRoute] string selector
        )
        {
            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            // We return not found if the operation item does not exist.
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the result if it exists.
            JobItem job = await LoadJobAsync(jobId, operationId, _persistence);

            if (job is null)
            {
                return NotFound(new
                {
                    Error = "Result with specified identifier could not be found.",
                    Id = operationId
                });
            }
            else
            {
                if (field is not null)
                {
                    job.Result = ((Dictionary<string, object>)job.Result)[field];
                    return job;
                }
                else
                {
                    return job;
                }
            }
            // TODO: Implement selector optional parameter support.
        }
        [HttpPost("{operationId}/upload/{jobId}/{field}")]
        public async Task<ActionResult<JobItem>> UploadFileIntoOperation(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string field,
            [FromForm] IFormFile file
        )
        {
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);

            OperationContext context = new()
            {
                Parent = null,
                Operation = operation,

                Default = new Dictionary<string, object>(operationItem.Default),
                Input = new Dictionary<string, object>(jobItem.Value),
                Output = new Dictionary<string, object>(),
            };
            SaveFile(file.OpenReadStream());


            jobItem.Tasks = new List<OperationTask>(operation.GetTasks(context));

            await SaveJobAsync(jobItem, _persistence);
            TaskCollection.Push((serviceScopeFactory) =>
            {
                return (jobItem, operation, context);
            });
            return Ok(jobItem);
        }
        #endregion
        #region Endpoints (Schema)
        [HttpGet("{operationId}/schema/{side}")]
        public async Task<ActionResult<JsonSchema>> ComputeOperationSchema(
            [FromRoute] string operationId,
            [FromRoute] OperationSide side
        )
        {
            // TODO: Reimplement.
            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            // We return "Not Found (404)" if the operation item does not exist.
            if (operationItem is null) return NotFound();

            if (operationItem.Type == "workflow")
            {
                WorkflowItem workflow = await WorkflowsController.LoadWorkflowAsync(operationItem.Subtype, _persistence);
                WorkflowOperation workflowOperation = (WorkflowOperation)await InstantiateOperation(operationItem, _persistence);
                JsonSchema schema = new JsonSchema();
                schema.Type = JsonObjectType.Object;
                if (side == OperationSide.Input)
                {
                    foreach (string inputField in workflowOperation.GetInputFields())
                    {
                        // TODO: Refactor copying of a schema to another schema object.
                        // TODO: We will need some more sophisticated schema generation for more complex workflows.
                        JsonSchemaProperty jsonProperty = new JsonSchemaProperty() { Title = inputField };
                        JsonSchemaGeneratorSettings jsonSettings = new JsonSchemaGeneratorSettings()
                        {
                            GenerateAbstractSchemas = false,
                            FlattenInheritanceHierarchy = true
                        };
                        Type inputType = workflowOperation.GetInputFieldType(inputField);
                        JsonSchema jsonSchema;
                        if (inputType.IsAssignableTo(typeof(Stream)))
                        {
                            jsonSchema = new JsonSchema();
                            jsonSchema.Type = JsonObjectType.File;
                        }
                        else
                        {
                            jsonSchema = JsonSchema.FromType(
                                workflowOperation.GetInputFieldType(inputField),
                                jsonSettings
                            );
                        }

                        foreach (PropertyInfo property in typeof(JsonSchema).GetProperties())
                        {
                            MethodInfo setMethod = property.GetSetMethod();
                            if (setMethod is not null)
                                property.SetValue(jsonProperty, property.GetValue(jsonSchema));
                        }

                        foreach (KeyValuePair<string, JsonSchema> entry in jsonSchema.Definitions)
                            jsonProperty.Definitions.Add(entry.Key, entry.Value);
                        foreach (KeyValuePair<string, JsonSchemaProperty> entry in jsonSchema.Properties)
                            jsonProperty.Properties.Add(entry.Key, entry.Value);
                        schema.Properties.Add(inputField, jsonProperty);
                    }
                }
                if (side == OperationSide.Output)
                {
                    foreach (string outputField in workflowOperation.GetOutputFields())
                    {
                        JsonSchemaProperty jsonProperty = new JsonSchemaProperty() { Title = outputField };
                        JsonSchemaGeneratorSettings jsonSettings = new JsonSchemaGeneratorSettings()
                        {
                            GenerateAbstractSchemas = false,
                            FlattenInheritanceHierarchy = true
                        };
                        Type inputType = workflowOperation.GetOutputFieldType(outputField);
                        JsonSchema jsonSchema;
                        if (inputType.IsAssignableTo(typeof(Stream)))
                        {
                            jsonSchema = new JsonSchema();
                            jsonSchema.Type = JsonObjectType.File;
                        }
                        else
                        {
                            jsonSchema = JsonSchema.FromType(
                                workflowOperation.GetInputFieldType(outputField),
                                jsonSettings
                            );
                        }

                        foreach (PropertyInfo property in typeof(JsonSchema).GetProperties())
                        {
                            MethodInfo setMethod = property.GetSetMethod();
                            if (setMethod is not null)
                                property.SetValue(jsonProperty, property.GetValue(jsonSchema));
                        }

                        foreach (KeyValuePair<string, JsonSchema> entry in jsonSchema.Definitions)
                            jsonProperty.Definitions.Add(entry.Key, entry.Value);
                        foreach (KeyValuePair<string, JsonSchemaProperty> entry in jsonSchema.Properties)
                            jsonProperty.Properties.Add(entry.Key, entry.Value);
                        schema.Properties.Add(outputField, jsonProperty);
                    }
                }
                return new ContentResult
                {
                    Content = schema.ToJson(),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            else
            {
                // Get the appropriate type.
                Type operationType = Operation.TypeFromName(operationItem.Type);
                Type baseType = operationType.BaseType;
                Type schemaType = null;
                switch (side)
                {
                    case OperationSide.Input:
                        schemaType = baseType.GetGenericArguments()[0];
                        break;
                    case OperationSide.Output:
                        schemaType = baseType.GetGenericArguments()[1];
                        break;
                }

                JsonSchema schema = JsonSchema.FromType(schemaType);
                return new ContentResult
                {
                    Content = schema.ToJson(),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
        }
        #endregion

        public static async Task<Operation> InstantiateOperation(OperationItem operationItem, Persistence _persistence)
        {
            if (operationItem.Type == "workflow")
            {
                WorkflowItem workflowItem = await WorkflowsController.LoadWorkflowAsync(operationItem.Subtype, _persistence);
                List<Operation> suboperations = new(workflowItem.Operations.Length);
                foreach (string suboperationId in workflowItem.Operations)
                {
                    suboperations.Add(
                        await InstantiateOperation(
                            await LoadOperationAsync(suboperationId.ToString(), _persistence),
                            _persistence
                        )
                    );
                }
                WorkflowOperationConnection[] connections = workflowItem.Connections.Select(item => new WorkflowOperationConnection()
                {
                    Source = new WorkflowOperationConnectionPoint()
                    {
                        Operation = item.Source.Operation,
                        Field = item.Source.Field
                    },
                    Target = new WorkflowOperationConnectionPoint()
                    {
                        Operation = item.Target.Operation,
                        Field = item.Target.Field
                    },
                    Multiplexer = item.Multiplex
                }).ToArray();

                WorkflowOperation operation = new(suboperations.ToArray(), connections)
                {
                    Identifier = operationItem.Id
                };
                operation.Default = operationItem.Default;

                return operation;
            }
            else
            {
                // TODO: Clean up and make private init members private init again.
                Type operationType = Operation.TypeFromName(operationItem.Type);

                Operation operation = (Operation)Activator.CreateInstance(operationType);
                operation.Identifier = operationItem.Id;
                operation.Default = operationItem.Default;

                return operation;
            }
        }
    }
}