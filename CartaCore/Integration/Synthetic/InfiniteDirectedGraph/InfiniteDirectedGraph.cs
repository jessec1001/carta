using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MorseCode.ITask;
using CartaCore.Graphs;
using CartaCore.Statistics;
using CartaCore.Graphs.Components;

namespace CartaCore.Integration.Synthetic
{
    /// <summary>
    /// Represents graph data of a random, (practically) infinite, directed graph. Both the vertices and edges are
    /// randomly generated and connected.
    /// </summary>
    public class InfiniteDirectedGraph :
        Graph,
        IRootedComponent,
        IDynamicLocalComponent<Vertex, Edge>,
        IDynamicOutComponent<Vertex, Edge>,
        IParameterizedComponent<InfiniteDirectedGraphParameters>
    {
        /// <inheritdoc />
        public InfiniteDirectedGraphParameters Parameters { get; set; }

        /// <summary>
        /// A map of name-type pairs of the names and types of each property included in this graph.
        /// </summary>
        private IDictionary<string, Type> NamedPropertyTypes { get; set; }

        /// <summary>
        /// Creates a new random sampled, infinite, directed graph with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters to generate the graph with.</param>
        public InfiniteDirectedGraph(InfiniteDirectedGraphParameters parameters = default)
            : base(nameof(InfiniteDirectedGraph))
        {
            // Set the parameters.
            Parameters = parameters;

            // Generate our random properties.
            CompoundRandom random = new(Parameters.Seed);
            int propertyCount = Parameters.PropertyCount;
            NamedPropertyTypes = new Dictionary<string, Type>();
            while (NamedPropertyTypes.Count < propertyCount)
            {
                string key = random.NextPsuedoword();
                Type type = GenerateRandomType(random);

                if (!NamedPropertyTypes.ContainsKey(key))
                    NamedPropertyTypes.Add(key, type);
            }

            // Initialize the graph components.
            Components.AddTop<IRootedComponent>(this);
            Components.AddTop<IDynamicLocalComponent<Vertex, Edge>>(this);
            Components.AddTop<IDynamicOutComponent<Vertex, Edge>>(this);
            Components.AddTop<IParameterizedComponent<InfiniteDirectedGraphParameters>>(this);
        }

        /// <summary>
        /// Generates a random type for a property.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>An available type for random data.</returns>
        protected static Type GenerateRandomType(CompoundRandom random)
        {
            // These are the available types to use in the graph vertex properties.
            Type[] availableTypes = new Type[]
            {
                typeof(int),
                typeof(double),
                typeof(string)
            };

            // Return the random types.
            return availableTypes[random.NextInt(availableTypes.Length)];
        }
        /// <summary>
        /// Generates a random value given an associated type.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="type">The type of value to generate.</param>
        /// <returns>The random value.</returns>
        protected static object GenerateRandomValue(CompoundRandom random, Type type)
        {
            // Return a random value based on the type desired.
            if (typeof(int) == type)
                return random.NextInt(0, 10) + 1;   // Random integer in [1, 10].
            if (typeof(double) == type)
                return random.NextDouble();         // Random double in [0, 1).
            if (typeof(string) == type)
                return random.NextPsuedoword();     // Random word with around 3 syllables.

            // If no types match, return null.
            // This should never happen.
            return null;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<string> Roots()
        {
            yield return await Task.FromResult(new CompoundRandom(Parameters.Seed).NextGuid().ToString());
        }

        /// <summary>
        /// Generates a requested vertex based on its identifier and the graph parameters.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>The requested vertex generated randomly.</returns>
        protected Vertex GenerateVertex(string id)
        {
            // Check that we can convert the specified ID to a GUID.
            if (!Guid.TryParse(id, out Guid guid)) throw new InvalidCastException();

            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new(Parameters.Seed, guid);

            // Create properties from the set of graph properties.
            Dictionary<string, IProperty> properties = new(capacity: NamedPropertyTypes.Count);
            foreach (KeyValuePair<string, Type> namedPropertyType in NamedPropertyTypes)
            {
                if (random.NextDouble() < Parameters.PropertyInclusionProbability)
                {
                    // Add the observation value to the property.
                    object value = GenerateRandomValue(random, namedPropertyType.Value);
                    Property property = new(value);
                    properties[namedPropertyType.Key] = property;
                }
            }
            properties.TrimExcess();

            // Create out-edges.
            int childCount = Parameters.ChildCount;
            List<Edge> outEdges = new(capacity: childCount);
            for (int index = 0; index < childCount; index++)
            {
                // We must construct the ID from the random number generator and not from the system.
                Guid randomId = random.NextGuid();

                // The source of each edge is the selected vertex.
                outEdges.Add(new Edge(id, randomId.ToString()) { Directed = true });
            }

            // Return the randomly generated vertex with properties.
            Vertex vertex = new(id, properties, outEdges);
            if (Parameters.Labeled) vertex.Label = random.NextPsuedoword();
            return vertex;
        }

        /// <inheritdoc />
        public ITask<Vertex> GetVertex(string id)
        {
            return Task.FromResult(GenerateVertex(id)).AsITask();
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetChildVertices(string id)
        {
            Vertex vertex = await GetVertex(id);
            foreach (Edge outEdge in vertex.OutEdges)
                yield return await GetVertex(outEdge.Target);
        }
    }
}