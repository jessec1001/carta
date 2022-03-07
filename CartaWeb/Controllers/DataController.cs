using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CartaCore.Graphs;
using CartaCore.Integration.Synthetic;
using CartaCore.Persistence;
using CartaCore.Serialization.Json;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;
using CartaWeb.Serialization.Json;
using CartaCore.Operations;
using Microsoft.AspNetCore.Authorization;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Serves data resources from multiple sources in a graph format. Each data source is generally a repository of
    /// many data resources. Each data resource has a unique identifier distinguishing it from other resources in the
    /// same data source. The data source and resource identifiers are required for accessing graph data. The default
    /// graph format returned by this API can be directly imported into Vis.js for ease of use in client applications.
    /// To specify a different format, use the "Accept" header to specify the desired format.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly Persistence _persistence;

        private static readonly Dictionary<DataSource, IDataResolver> DataResolvers;

        /// <inheritdoc />
        public DataController(ILogger<DataController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        #region Persistence
        private static readonly string BaseDirectory = @"data";
        private static readonly string GraphDirectory = @"graphs";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        static DataController()
        {
            // Create the data resolvers.
            DataResolvers = new Dictionary<DataSource, IDataResolver>
            {
                {
                    DataSource.Synthetic,
                    new OptionsDataResolver
                    (
                        new Dictionary<string, OptionsResourceResolver>
                        (
                            new Dictionary<string, OptionsResourceResolver>
                            {
                                [nameof(FiniteUndirectedGraph)] = new OptionsResourceResolver<FiniteUndirectedGraphParameters>
                                    (options => new FiniteUndirectedGraph(options))
                                    { Operation = new FiniteUndirectedGraphOperation().GetTemplate(new FiniteUndirectedGraphOperationIn()) },
                                [nameof(InfiniteDirectedGraph)] = new OptionsResourceResolver<InfiniteDirectedGraphParameters>
                                    (options => new InfiniteDirectedGraph(options))
                                    { Operation = new InfiniteDirectedGraphOperation().GetTemplate(new InfiniteDirectedGraphOperationIn()) },
                            },
                            StringComparer.OrdinalIgnoreCase
                        )
                    )
                },
                { DataSource.HyperThought, new HyperthoughtDataResolver() },
                { DataSource.User, new UserDataResolver() }
            };

            // JSON options.
            JsonOptions.Converters.Insert(0, new JsonObjectConverter());
        }

        /// <summary>
        /// Loads all of the stored graphs asynchronously.
        /// </summary>
        /// <returns>A list of all stored graphs.</returns>
        public static async Task<List<MemoryGraph>> LoadGraphsAsync()
        {
            // Read all of the files.
            string graphDir = Path.Combine(BaseDirectory, GraphDirectory);
            if (!Directory.Exists(graphDir)) return new List<MemoryGraph>();
            return await Directory.GetFiles(graphDir)
                .Select(filePath => int.Parse(Path.GetFileNameWithoutExtension(filePath)))
                .ToAsyncEnumerable()
                .SelectAwait(async id => await LoadGraphAsync(id))
                .ToListAsync();
        }
        /// <summary>
        /// Loads a single graph asynchronously specified by an identifier.
        /// </summary>
        /// <param name="id">The identifier of the graph to get.</param>
        /// <returns>
        /// The loaded graph or <c>null</c> if there is no graph corresponding to the specified identifier.
        /// </returns>
        public static async Task<MemoryGraph> LoadGraphAsync(int id)
        {
            // Try to read the JSON file.
            string graphPath = Path.Combine(BaseDirectory, GraphDirectory, $"{id}.json");
            if (!System.IO.File.Exists(graphPath)) return null;
            using StreamReader file = new(graphPath);
            MemoryGraph graph = JsonSerializer.Deserialize<VisFormat>(await file.ReadToEndAsync(), JsonOptions).Graph;
            graph.Id = id.ToString();
            return graph;
        }
        /// <summary>
        /// Saves a single graph asynchronously. If an existing identifier is specified, the graph is overwritten.
        /// Otherwise, an unused identifier is chosen and the graph is saved there.
        /// </summary>
        /// <param name="graph">The graph object.</param>
        /// <param name="existingId">An existing graph identifier, if applicable.</param>
        /// <returns>The identifier of the saved graph.</returns>
        public static async Task<int> SaveGraphAsync(MemoryGraph graph, int? existingId = null)
        {
            // Find out what identifier we should assign to this graph.
            string graphDir = Path.Combine(BaseDirectory, GraphDirectory);
            Directory.CreateDirectory(graphDir);
            int id = 0;
            if (existingId.HasValue) id = existingId.Value;
            else while (System.IO.File.Exists(Path.Combine(graphDir, $"{id}.json"))) id++;

            // Create the JSON file.
            string graphPath = Path.Combine(graphDir, $"{id}.json");
            using (StreamWriter file = new(graphPath))
            {
                graph.Id = id.ToString();
                string json = JsonSerializer.Serialize(await VisFormat.CreateAsync(graph), JsonOptions);
                await file.WriteAsync(json);
            }

            // Return the identifier of the created graph.
            return id;
        }
        /// <summary>
        /// Deletes a single graph specified by an identifier.
        /// </summary>
        /// <param name="id">The identifier of the graph to delete.</param>
        public static async Task<bool> DeleteGraphAsync(int id)
        {
            // Try to delete the JSON file.
            string graphPath = Path.Combine(BaseDirectory, GraphDirectory, $"{id}.json");
            if (System.IO.File.Exists(graphPath))
            {
                System.IO.File.Delete(graphPath);
                return await Task.FromResult(true);
            }
            else return await Task.FromResult(false);
        }
        #endregion

        /// <summary>
        /// Finds the sample graph associated with a specified source and resource.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source</param>
        /// <returns>The graph data.</returns>
        private async Task<Graph> LookupData(DataSource source, string resource)
        {
            if (resource is not null && DataResolvers.TryGetValue(source, out IDataResolver resolver))
                return await resolver.GenerateGraphAsync(this, resource);
            return null;
        }

        #region Endpoints (Data CRUD)
        /// <summary>
        /// Gets the valid sources to access data from. Note that this includes data sources that the user may not have
        /// authorization to access. In querying the valid data sources, no data is actually retrieved.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">A list of the valid data source identifiers.</returns>
        [HttpGet]
        public ActionResult<IList<DataSource>> GetSources()
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.

            return Ok(Enum.GetValues<DataSource>().ToList());
        }
        /// <summary>
        /// Gets the valid resources located at a specific data source. This will query the data source with any
        /// provided authentication to determine available resources.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <request name="Synthetic">
        ///     <arg name="source">synthetic</arg>
        /// </request>
        /// <request name="HyperThought">
        ///     <arg name="source">hyperthought</arg>
        /// </request>
        /// <returns status="200">A list of resource identifiers.</returns>
        /// <returns status="404">
        /// Occurs when the specified data source is invalid or cannot be accessed with the provided authentication.
        /// </returns>
        [HttpGet("{source}")]
        public async Task<ActionResult<IList<string>>> GetResources(
            [FromRoute] DataSource source
        )
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.

            try
            {
                if (!DataResolvers.ContainsKey(source)) return NotFound();

                IList<string> resources = await DataResolvers[source].FindResourcesAsync(this);
                if (resources is null) return NotFound();
                else return Ok(resources);
            }
            catch (HttpRequestException requestException)
            {
                // Forward the HTTP exception to the caller.
                return StatusCode
                (
                    (int)requestException.StatusCode,
                    new { Source = source }
                );
            }
        }
        /// <summary>
        /// Creates a graph resource inside the specified data source. The graph data must be specified in the one of
        /// the supported formats. The graph will automatically be assigned a resource identifier that is returned from
        /// this function.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="graph">The specified graph data.</param>
        /// <returns status="200">
        /// The graph data that was created by the function. If any values were missing from the graph data,
        /// they are automatically assigned defaults.
        /// </returns>
        /// <returns status="400">Occurs when the specified data source does not allow data creation.</returns>
        [Authorize]
        [HttpPost("{source}")]
        public async Task<ActionResult<MemoryGraph>> PostGraph(
            [FromRoute] DataSource source,
            [FromBody] MemoryGraph graph
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to users with the appropriate permissions to
            //       the data resource. For now, until we add permissions for each dataset, every dataset is available.

            try
            {
                // TODO: Allow user-uploaded graphs to have aliases assigned to them.
                if (source == DataSource.User)
                {
                    int id = await SaveGraphAsync(graph);
                    graph.Id = id.ToString();
                    return Ok(graph);
                }
                return BadRequest();
            }
            catch (HttpRequestException requestException)
            {
                // Forward the HTTP exception to the caller.
                return StatusCode
                (
                    (int)requestException.StatusCode,
                    new { Source = source }
                );
            }
        }
        /// <summary>
        /// Deletes a graph resource specified by a data source and resource.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="resource">The data resource identifier.</param>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="400">Occurs when the specified data source does not allow data creation.</returns>
        /// <returns status="404">Occurs when the specified resource could not be found.</returns>
        [Authorize]
        [HttpDelete("{source}/{resource}")]
        public async Task<ActionResult> DeleteGraph(
            [FromRoute] DataSource source,
            [FromRoute] string resource
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to users with the appropriate permissions to
            //       the data resource. For now, until we add permissions for each dataset, every dataset is available.

            try
            {
                if (source == DataSource.User)
                {
                    if (!int.TryParse(resource, out int id)) return BadRequest();
                    if (await DeleteGraphAsync(id))
                        return Ok();
                    else
                        return NotFound();
                }
                return BadRequest();
            }
            catch (HttpRequestException requestException)
            {
                // Forward the HTTP exception to the caller.
                return StatusCode
                (
                    (int)requestException.StatusCode,
                    new { Source = source, Resource = resource }
                );
            }
        }
        #endregion

        #region Endpoints (Data Queries)
        /// <summary>
        /// Gets a particular selector of data from the graph at the specified data resource.
        /// </summary>
        /// <request name="Synthetic Graph - Root">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="selector">roots</arg>
        /// </request>
        /// <request name="HyperThought Workflow Graph - Root">
        ///     <arg name="source">hyperthought</arg>
        ///     <arg name="resource">Sandbox.Test01</arg>
        ///     <arg name="selector">roots</arg>
        /// </request>
        /// <request name="Synthetic Graph 1 - Specific Vertex">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="selector">include</arg>
        ///     <arg name="ids">123e4567-e89b-12d3-a456-426614174000</arg>
        /// </request>
        /// <request name="Synthetic Graph 2 - Specific Vertex">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="selector">include</arg>
        ///     <arg name="ids">fd712a01-ac32-47d1-05fd-a619c088dbe1</arg>
        ///     <arg name="seed">1234</arg>
        /// </request>
        /// <request name="Synthetic Graph - No Children">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="selector">children</arg>
        ///     <arg name="ids">123e4567-e89b-12d3-a456-426614174000</arg>
        /// </request>
        /// <request name="Synthetic Graph - Children">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="selector">children</arg>
        ///     <arg name="ids">fd712a01-ac32-47d1-05fd-a619c088dbe1</arg>
        ///     <arg name="seed">0</arg>
        /// </request>
        /// <returns status="200">
        /// The graph containing the selected vertices and edges constructed from the data resource.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the data resource cannot be queried using the specified selector.
        /// </returns>
        /// <returns status="404">
        /// Occurs if the data resource or source could not be found or cannot be accessed with the provided
        /// authentication.
        /// </returns>
        [Authorize]
        [HttpGet("{source}/{resource}/{selector}")]
        public async Task<ActionResult<Graph>> GetGraphSelection(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromRoute] string selector
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to users with the appropriate permissions to
            //       the data resource. For now, until we add permissions for each dataset, every dataset is available.

            try
            {
                // Get the base graph and check if it was actually retrieved.
                Graph graph = await LookupData(source, resource);
                if (graph is null) return NotFound();

                // We find the appropriate selector class corresponding to the specified discriminant.
                Type selectorType = OperationHelper.FindSelectorType(selector);
                if (selectorType is null) return BadRequest();

                // We wrap our original graph in this selector graph to provide selection-specific data retrieval.
                ISelector<Graph, Graph> selectorOperation =
                    OperationHelper.ConstructSelector<Graph, Graph>(selectorType, out object selectorParameters);
                await TryUpdateModelAsync(selectorParameters, selectorParameters.GetType(), "selector");
                graph = await selectorOperation.Select(graph, selectorParameters);

                // Return the graph with an okay status.
                return Ok(graph);
            }
            catch (HttpRequestException requestException)
            {
                // Forward the HTTP exception to the caller.
                return StatusCode
                (
                    (int)requestException.StatusCode,
                    new { Source = source, Resource = resource }
                );
            }
        }

        /// <summary>
        /// Gets an operation template that may used to construct an operation instance. This operation can be used to
        /// retrieve the data resource.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="resource">The data resource identifier.</param>
        /// <returns status="200">An operation template.</returns>
        /// <returns status="404">Occurs when the specified data source is invalid.</returns>
        [Authorize]
        [HttpGet("{source}/{resource}/operation")]
        public async Task<ActionResult<OperationTemplate>> GetDataOperation(
            [FromRoute] DataSource source,
            [FromRoute] string resource
        )
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.
            //       For now, until we add permissions on the toolbox level, every operation type is available.

            try
            {
                if (!DataResolvers.ContainsKey(source)) return NotFound();

                OperationTemplate template = await DataResolvers[source].GenerateOperation(this, resource);
                return Ok(template);
            }
            catch (HttpRequestException requestException)
            {
                return StatusCode
                (
                    (int)requestException.StatusCode,
                    new { Source = source, Resource = resource }
                );
            }
        }
        #endregion
    }
}