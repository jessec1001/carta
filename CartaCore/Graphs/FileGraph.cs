using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CartaCore.Graphs.Components;
using CartaCore.Persistence;
using CartaCore.Serialization.Json;
using MorseCode.ITask;

namespace CartaCore.Graphs
{
    // TODO: Implement rooted behavior.
    /// <summary>
    /// Saves/loads a graph to/from a file dynamically.
    /// </summary>
    /// <typeparam name="TVertex">The type of vertex.</typeparam>
    /// <typeparam name="TEdge">The type of edge.</typeparam>
    public class FileGraph<TVertex, TEdge> : Graph,
        IRootedComponent,
        IEnumerableComponent<TVertex, TEdge>,
        IDynamicLocalComponent<TVertex, TEdge>,
        IDynamicInComponent<TVertex, TEdge>,
        IDynamicOutComponent<TVertex, TEdge>
        where TVertex : IVertex<TEdge>
        where TEdge : IEdge
    {
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// The path to the graph directory.
        /// </summary>
        public string GraphPath { get; private init; }
        /// <summary>
        /// The amount of time to delay while waiting for a locked file.
        /// If set to <c>null</c>, no delay will be used and an exception will be thrown.
        /// </summary>
        public int? LockDelay { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileGraph{TVertex, TEdge}"/> class.
        /// </summary>
        /// <param name="path">The path to the graph directory.</param>
        /// <param name="id">The identifier for the graph.</param>
        /// <param name="properties">The properties of the graph.</param>
        public FileGraph(
            string path,
            string id,
            IDictionary<string, IProperty> properties
        ) : base(id, properties)
        {
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            _jsonOptions.IgnoreNullValues = true;
            _jsonOptions.Converters.Add(new JsonObjectConverter());
            _jsonOptions.Converters.Add(new JsonPropertyConverter());

            // Set the instance parameters.
            GraphPath = path;
            LockDelay = null;

            // Initialize all of the components.
            Components.AddTop<IRootedComponent>(this);
            Components.AddTop<IEnumerableComponent<TVertex, TEdge>>(this);
            Components.AddTop<IDynamicLocalComponent<TVertex, TEdge>>(this);
            Components.AddTop<IDynamicInComponent<TVertex, TEdge>>(this);
            Components.AddTop<IDynamicOutComponent<TVertex, TEdge>>(this);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FileGraph{TVertex, TEdge}"/> class.
        /// </summary>
        /// <param name="path">The path to the graph directory.</param>
        /// <param name="id">The identifier for the graph.</param>
        public FileGraph(
            string path,
            string id
        ) : this(path, id, new Dictionary<string, IProperty>()) { }

        /// <summary>
        /// Adds the specified vertex to the graph.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        /// <returns><c>true</c> if the vertex was successfully added; <c>false</c> otherwise.</returns>
        public async Task<bool> AddVertex(TVertex vertex)
        {
            // Add a vertex file.
            string vertexPath = Path.Combine(GraphPath, $"{vertex.Id}.json");
            string vertexJson = JsonSerializer.Serialize(vertex, _jsonOptions);
            MemoryStream vertexStream = new(Encoding.UTF8.GetBytes(vertexJson));
            return await FileHelper.WriteFileAsync(vertexPath, vertexStream, LockDelay);
        }
        /// <summary>
        /// Deletes the specified vertex from the graph.
        /// </summary>
        /// <param name="vertex">The vertex to delete.</param>
        /// <returns><c>true</c> if the vertex was successfully deleted; <c>false</c> otherwise.</returns>
        public async Task<bool> RemoveVertex(TVertex vertex)
        {
            // Delete a vertex file if it exists.
            string vertexPath = Path.Combine(GraphPath, $"{vertex.Id}.json");
            return await FileHelper.DeleteFileAsync(vertexPath, LockDelay);
        }

        /// <summary>
        /// Clears all of the vertices and edges stored in the graph.
        /// </summary>
        public async Task Clear()
        {
            // We delete all of the files in the graph directory.
            string[] files = Directory.GetFiles(GraphPath);
            foreach (string file in files)
            {
                await FileHelper.DeleteFileAsync(file, LockDelay);
            }
        }

        /// <summary>
        /// Sets the root vertex identifiers. 
        /// </summary>
        /// <param name="roots">The root vertex identifiers.</param>
        public async Task SetRoots(IAsyncEnumerable<string> roots)
        {
            // We need to create a roots file.
            List<string> rootList = await roots.ToListAsync();
            string rootsPath = Path.Combine(GraphPath, "roots.json");
            string rootsJson = JsonSerializer.Serialize(rootList, _jsonOptions);
            MemoryStream rootsStream = new(Encoding.UTF8.GetBytes(rootsJson));
            await FileHelper.WriteFileAsync(rootsPath, rootsStream, LockDelay);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TVertex> GetVertices()
        {
            // We fetch the list of files in the directory.
            IEnumerable<string> files = Directory.GetFiles(GraphPath);

            // We iterate over each file and deserialize the vertex.
            foreach (string file in files)
            {
                // We deserialize the vertex.
                FileStream stream = await FileHelper.ReadFileAsync(file, LockDelay);
                TVertex vertex = await JsonSerializer.DeserializeAsync<TVertex>(stream, _jsonOptions);
                stream.Dispose();

                // We yield the vertex.
                yield return vertex;
            }
        }

        /// <inheritdoc />
        public async ITask<TVertex> GetVertex(string id)
        {
            // We read in the vertex from the file.
            FileStream stream = await FileHelper.ReadFileAsync(Path.Join(GraphPath, $"{id}.json"), LockDelay);
            TVertex vertex = await JsonSerializer.DeserializeAsync<TVertex>(stream, _jsonOptions);
            stream.Dispose();
            return vertex;
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<TVertex> GetParentVertices(string id)
        {
            // Fetch the vertex from the graph and yield its parents.
            TVertex vertex = await GetVertex(id);
            foreach (TEdge edge in vertex.Edges)
            {
                if (edge.Target == id)
                    yield return await GetVertex(edge.Source);
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<TVertex> GetChildVertices(string id)
        {
            // Fetch the vertex from the graph and yield its children.
            TVertex vertex = await GetVertex(id);
            foreach (TEdge edge in vertex.Edges)
            {
                if (edge.Source == id)
                    yield return await GetVertex(edge.Target);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<string> Roots()
        {
            string rootsPath = Path.Combine(GraphPath, "roots.json");
            FileStream rootsStream = await FileHelper.ReadFileAsync(rootsPath, LockDelay);
            string[] roots = await JsonSerializer.DeserializeAsync<string[]>(rootsStream, _jsonOptions);
            rootsStream.Dispose();
            foreach (string root in roots)
                yield return root;
        }
    }
}