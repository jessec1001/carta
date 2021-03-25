using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CartaCore.Data;
using CartaCore.Data.Synthetic;
using CartaCore.Workflow;
using CartaWeb.Models.Data;
using CartaWeb.Serialization.Json;

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

        private static Dictionary<DataSource, IDataResolver> DataResolvers;

        private static string BaseDirectory = @"data";
        private static string GraphDirectory = @"graphs";

        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        static DataController()
        {
            // Create the data resolvers.
            DataResolvers = new Dictionary<DataSource, IDataResolver>();
            DataResolvers.Add(DataSource.Synthetic, new OptionsDataResolver
            (
                new Dictionary<string, OptionsResourceResolver>
                (
                    new Dictionary<string, OptionsResourceResolver>
                    {
                        [nameof(FiniteUndirectedGraph)] = new OptionsResourceResolver<FiniteUndirectedGraphParameters>
                            (options => new FiniteUndirectedGraph(options)),
                        [nameof(InfiniteDirectedGraph)] = new OptionsResourceResolver<InfiniteDirectedGraphParameters>
                            (options => new InfiniteDirectedGraph(options)),
                    },
                    StringComparer.OrdinalIgnoreCase
                )
            ));
            DataResolvers.Add(DataSource.HyperThought, new HyperthoughtDataResolver());
            DataResolvers.Add(DataSource.User, new UserDataResolver());
        }

        /// <summary>
        /// Loads all of the stored graphs asynchronously.
        /// </summary>
        /// <returns>A list of all stored graphs.</returns>
        public static async Task<List<FiniteGraph>> LoadGraphsAsync()
        {
            // Read all of the files.
            string graphDir = Path.Combine(BaseDirectory, GraphDirectory);
            if (!Directory.Exists(graphDir)) return new List<FiniteGraph>();
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
        public static async Task<FiniteGraph> LoadGraphAsync(int id)
        {
            // Try to read the JSON file.
            string graphPath = Path.Combine(BaseDirectory, GraphDirectory, $"{id}.json");
            if (!System.IO.File.Exists(graphPath)) return null;
            using (StreamReader file = new StreamReader(graphPath))
            {
                FiniteGraph graph = JsonSerializer.Deserialize<VisFormat>(await file.ReadToEndAsync(), JsonOptions).Graph;
                graph.Identifier = Identity.Create(id);
                return graph;
            }
        }
        /// <summary>
        /// Saves a single graph asynchronously. If an existing identifier is specified, the graph is overwritten.
        /// Otherwise, an unused identifier is chosen and the graph is saved there.
        /// </summary>
        /// <param name="graph">The graph object.</param>
        /// <param name="existingId">An existing graph identifier, if applicable.</param>
        /// <returns>The identifier of the saved graph.</returns>
        public static async Task<int> SaveGraphAsync(FiniteGraph graph, int? existingId = null)
        {
            // Find out what identifier we should assign to this graph.
            string graphDir = Path.Combine(BaseDirectory, GraphDirectory);
            Directory.CreateDirectory(graphDir);
            int id = 0;
            if (existingId.HasValue) id = existingId.Value;
            else while (System.IO.File.Exists(Path.Combine(graphDir, $"{id}.json"))) id++;

            // Create the JSON file.
            string graphPath = Path.Combine(graphDir, $"{id}.json");
            using (StreamWriter file = new StreamWriter(graphPath))
            {
                graph.Identifier = Identity.Create(id);
                string json = JsonSerializer.Serialize<VisFormat>(await VisFormat.CreateAsync(graph), JsonOptions);
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


        /// <inheritdoc />
        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Finds the sample graph associated with a specified source and resource.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="resource">The resource located on the data source</param>
        /// <returns>The graph data.</returns>
        private async Task<Graph> LookupData(DataSource source, string resource)
        {
            if (resource is not null && DataResolvers.TryGetValue(source, out IDataResolver resolver))
                return await resolver.GenerateAsync(this, resource);
            return null;
        }

        /// <summary>
        /// Gets the valid sources to access data from. Note that this includes data sources that the user may not have
        /// authorization to access. In querying the valid data sources, no data is actually retrieved.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">A list of the valid data source identifiers.</returns>
        [HttpGet]
        public ActionResult<IList<DataSource>> GetSources()
        {
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
            if (!DataResolvers.ContainsKey(source)) return NotFound();

            IList<string> resources = await DataResolvers[source].FindResourcesAsync(this);
            if (resources is null) return NotFound();
            else return Ok(resources);
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
        [HttpPost("{source}")]
        public async Task<ActionResult<FiniteGraph>> PostGraph(
            [FromRoute] DataSource source,
            [FromBody] FiniteGraph graph
        )
        {
            if (source == DataSource.User)
            {
                int id = await SaveGraphAsync(graph);
                graph.Identifier = Identity.Create(id);
                return Ok(graph);
            }
            return BadRequest();
        }
        /// <summary>
        /// Deletes a graph resource specified by a data source and resource.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="resource">The data resource identifier.</param>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="400">Occurs when the specified data source does not allow data creation.</returns>
        /// <returns status="404">Occurs when the specified resource could not be found.</returns>
        [HttpDelete("{source}/{resource}")]
        public async Task<ActionResult> DeleteGraph(
            [FromRoute] DataSource source,
            [FromRoute] string resource
        )
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

        /// <summary>
        /// Gets the base graph for the specified data resource. If the resource contains infinite or directed data, the
        /// base graph only contains a singular root vertex. Use the other Data API functionality to extract more
        /// information from these types of graphs. If the resource contains finite or undirected data, the base graph
        /// contains all of the vertices and edges. The graph may have an optional workflow, specified by the Workflow
        /// API, applied to it.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="resource">The data resource identifier located on the data source.</param>
        /// <param name="workflowId">The identifier of the workflow to be applied to the graph.</param>
        /// <request name="Synthetic Graph">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        /// </request>
        /// <request name="Synthetic Graph with Workflow">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="workflow">0</arg>
        /// </request>
        /// <request name="HyperThought Workflow Graph">
        ///     <arg name="source">hyperthought</arg>
        ///     <arg name="resource">Sandbox.Test01</arg>
        /// </request>
        /// <returns status="200">
        /// The base graph containing vertices and edges constructed from the data resource.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the data resource cannot be represented as a finite nor dynamic graph.
        /// </returns>
        /// <returns status="404">
        /// Occurs if the data resource or source could not be found or cannot be accessed with the provided
        /// authentication.
        /// </returns>
        [HttpGet("{source}/{resource}")]
        public async Task<ActionResult<FiniteGraph>> GetGraph(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery(Name = "workflow")] int? workflowId = null
        )
        {
            Graph graph = await LookupData(source, resource);

            // If we could not find the resource, we should return a not found response.
            if (graph is null) return NotFound();

            // We return a response based on the type of graph.
            FiniteGraph subgraph;
            switch (graph)
            {
                case IDynamicOutGraph<IOutVertex> outGraph:
                    // Return the subgraph of the graph containing the base vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(outGraph, new[] { outGraph.BaseIdentifier });
                    break;
                case IDynamicGraph<Vertex> dynGraph:
                    // Return the subgraph of the graph containing the base vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(dynGraph, new[] { dynGraph.BaseIdentifier });
                    break;
                case FiniteGraph finiteGraph:
                    // Return entire graph.
                    subgraph = finiteGraph;
                    break;
                default:
                    // If a graph does not match a previous type, we cannot retrieve it.
                    return BadRequest();
            }

            // We apply a workflow if requested.
            if (workflowId.HasValue)
            {
                Workflow workflow = await WorkflowController.LoadWorkflowAsync(workflowId.Value);
                subgraph = await workflow.ApplyAsync(subgraph);
            }

            return Ok(subgraph);
        }

        /// <summary>
        /// Gets a graph containing only a particular vertex for the specified data resource. The data source must
        /// support queryable vertices by unique identifiers for this functionality to apply. The vertex will contain
        /// all of its properties and observations. Additionally, all of the in-edges or out-edges, depending on the
        /// capabilities of the data source, will be included in the produced graph. The graph may have an optional
        /// workflow, specified by the Workflow API, applied to it.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="resource">The data resource identifier located on the data source.</param>
        /// <param name="id">The unique identifier of the vertex.</param>
        /// <param name="workflowId">The identifier of the workflow to be applied to the graph.</param>
        /// <request name="Synthetic Graph 1">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="id">123e4567-e89b-12d3-a456-426614174000</arg>
        /// </request>
        /// <request name="Synthetic Graph 2">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="id">fd712a01-ac32-47d1-05fd-a619c088dbe1</arg>
        ///     <arg name="seed">1234</arg>
        /// </request>
        /// <request name="Synthetic Graph 2 with Workflow">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="id">fd712a01-ac32-47d1-05fd-a619c088dbe1</arg>
        ///     <arg name="seed">1234</arg>
        ///     <arg name="workflow">0</arg>
        /// </request>
        /// <returns status="200">
        /// A graph with the requested vertex loaded with its properties and observations and its corresponding edges.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the data resource cannot be queried for a specific vertex.
        /// </returns>
        /// <returns status="404">
        /// Occurs if the data resource or source could not be found or cannot be accessed with the provided
        /// authentication.
        /// </returns>
        [HttpGet("{source}/{resource}/props")]
        public async Task<ActionResult<FiniteGraph>> GetProperties(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] string id,
            [FromQuery(Name = "workflow")] int? workflowId = null
        )
        {
            Graph graph = await LookupData(source, resource);

            // If we could not find the resource, we should return a not found response.
            if (graph is null) return NotFound();

            // We return a response based on the type of graph.
            FiniteGraph subgraph;
            switch (graph)
            {
                case IDynamicOutGraph<IOutVertex> outGraph:
                    // Return the subgraph of the graph containing the requested vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(outGraph, new[] { Identity.Create(id) });
                    break;
                case IDynamicGraph<Vertex> dynGraph:
                    // Return the subgraph of the graph containing the base vertex.
                    subgraph = await FiniteGraph.CreateSubgraph(dynGraph, new[] { Identity.Create(id) });
                    break;
                default:
                    // If a graph does not match a previous type, we cannot retrieve it.
                    return BadRequest();
            }

            // We apply a workflow if requested.
            if (workflowId.HasValue)
            {
                Workflow workflow = await WorkflowController.LoadWorkflowAsync(workflowId.Value);
                subgraph = await workflow.ApplyAsync(subgraph);
            }

            return Ok(subgraph);
        }

        /// <summary>
        /// Gets a graph containing the children of a particular vertex for the specified data resource. The data source
        /// must support queryable vertices by unique identifiers and must support obtaining out-edges/children
        /// connections for a vertex for this functionality to apply. The loaded children vertices and all of their
        /// in-edges or out-edges, depending on the capabilities of the data source, will be included in the produced
        /// graph. The graph may have an optional workflow, specified by the Workflow API, applied to it.
        /// </summary>
        /// <param name="source">The data source identifier.</param>
        /// <param name="resource">The data resource identifier located on the data source.</param>
        /// <param name="id">The unique identifier of the vertex.</param>
        /// <param name="workflowId">The identifier of the workflow to be applied to the graph.</param>
        /// <request name="Synthetic Graph - No Children">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="id">123e4567-e89b-12d3-a456-426614174000</arg>
        /// </request>
        /// <request name="Synthetic Graph - Children">
        ///     <arg name="source">synthetic</arg>
        ///     <arg name="resource">infiniteDirectedGraph</arg>
        ///     <arg name="id">fd712a01-ac32-47d1-05fd-a619c088dbe1</arg>
        ///     <arg name="seed">0</arg>
        /// </request>
        /// <returns status="200">
        /// A graph with the children of the requested vertex and their corresponding edges. This is equivalent to
        /// aggregating individual calls to retrieve each of the child vertices of the requested vertex.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the data resource cannot be queries for a specific vertex or the out-edges cannot be obtained.
        /// </returns>
        /// <returns status="404">
        /// Occurs if the data resource or source could not be found or cannot be accessed with the provided
        /// authentication.
        /// </returns>
        [HttpGet("{source}/{resource}/children")]
        public async Task<ActionResult<Graph>> GetChildren(
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery] string id,
            [FromQuery(Name = "workflow")] int? workflowId = null
        )
        {
            Graph graph = await LookupData(source, resource);

            // If we could not find the resource, we should return a not found response.
            if (graph is null) return NotFound();

            // We return a response based on the type of graph.
            FiniteGraph subgraph;
            switch (graph)
            {
                case IDynamicOutGraph<InOutVertex> inoutGraph:
                    // Return the subgraph of the graph containing the requested vertex.
                    subgraph = await FiniteGraph.CreateChildSubgraph(inoutGraph, new[] { Identity.Create(id) });
                    break;
                case IDynamicOutGraph<OutVertex> outGraph:
                    // Return the subgraph of the graph containing the requested vertex.
                    subgraph = await FiniteGraph.CreateChildSubgraph(outGraph, new[] { Identity.Create(id) });
                    break;
                default:
                    // If a graph does not match a previous type, we cannot retrieve it.
                    return BadRequest();
            }

            // We apply a workflow if requested.
            if (workflowId.HasValue)
            {
                Workflow workflow = await WorkflowController.LoadWorkflowAsync(workflowId.Value);
                subgraph = await workflow.ApplyAsync(subgraph);
            }

            return Ok(subgraph);
        }
    }
}