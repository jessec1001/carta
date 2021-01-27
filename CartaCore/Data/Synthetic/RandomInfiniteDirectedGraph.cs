using System;
using System.Collections.Generic;

using QuikGraph;

using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    /// <summary>
    /// Represents graph data of a random, (practically) infinite, directed graph. Both the vertices and edges are
    /// randomly generated and connected.
    /// </summary>
    public class RandomInfiniteDirectedGraph : ISampledGraph, IOptionsGraph<RandomInfiniteDirectedGraphOptions>
    {
        /// <inheritdoc />
        public RandomInfiniteDirectedGraphOptions Options { get; set; }

        /// <summary>
        /// The set of properties and their types that vertices select from in the graph.
        /// </summary>
        private IDictionary<string, Type> Properties { get; set; }

        /// <summary>
        /// Creates a new random sampled, infinite, directed graph with the specified parameters.
        /// </summary>
        /// <param name="options">The options to generate the graph with.</param>
        public RandomInfiniteDirectedGraph(RandomInfiniteDirectedGraphOptions options)
        {
            // Set the options.
            Options = options;

            // Generate our random properties.
            CompoundRandom random = new CompoundRandom(Options.Seed);
            Properties = new Dictionary<string, Type>();
            while (Properties.Count < Options.PropertyCount)
            {
                string key = random.NextPsuedoword();
                Type type = GenerateRandomType(random);

                if (!Properties.ContainsKey(key))
                    Properties.Add(key, type);
            }
        }

        /// <summary>
        /// Generates a random type for a property.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>An available type for random data.</returns>
        protected Type GenerateRandomType(CompoundRandom random)
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
        protected object GenerateRandomValue(CompoundRandom random, Type type)
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

        /// <summary>
        /// Whether the graph has a finite or infinite number of vertices and edges.
        /// </summary>
        /// <value>Always <c>false</c>.</value>
        public bool IsFinite => false;

        /// <inheritdoc />
        public FreeformGraph GetEntire() => throw new NotFiniteNumberException();

        /// <inheritdoc />
        public FreeformVertex GetProperties(Guid id)
        {
            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new CompoundRandom(Options.Seed, id);

            // Construct a dictionary of random properties.
            SortedList<string, FreeformVertexProperty> properties = new SortedList<string, FreeformVertexProperty>();
            foreach (KeyValuePair<string, Type> property in Properties)
            {
                if (random.NextDouble() < Options.PropertyDensity)
                    properties.Add(property.Key, new FreeformVertexProperty
                    {
                        Type = property.Value,
                        Value = GenerateRandomValue(random, property.Value)
                    });
            }

            // Return the randomly generated vertex with properties.
            return new FreeformVertex
            {
                Id = id,
                Properties = properties
            };
        }
        /// <inheritdoc />
        public IEnumerable<Edge<FreeformVertex>> GetEdges(Guid id)
        {
            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new CompoundRandom(Options.Seed, id);
            FreeformVertex sourceVertex = new FreeformVertex { Id = id };

            // Keep constructing children until the random generator doesn't sample within the current probability.
            double probability = Options.ChildProbability;
            while (random.NextDouble() < probability)
            {
                // We must construct the ID from the random number generator and not from the system.
                Guid randomId = random.NextGuid();

                // The source of each edge is the selected vertex.
                yield return new Edge<FreeformVertex>(
                    sourceVertex,
                    new FreeformVertex { Id = randomId }
                );

                // We lower the probability by some amount each time.
                probability *= Options.ChildDampener;
            }
        }
    }
}