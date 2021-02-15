using System;
using System.Collections.Generic;
using System.Linq;

using CartaCore.Data.Freeform;
using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    /// <summary>
    /// Represents graph data of a random, (practically) infinite, directed graph. Both the vertices and edges are
    /// randomly generated and connected.
    /// </summary>
    public class InfiniteDirectedGraph : FreeformGraph, IParameterizedGraph<InfiniteDirectedGraphParameters>
    {
        /// <inheritdoc />
        public InfiniteDirectedGraphParameters Parameters { get; set; }

        /// <summary>
        /// The set of properties and their types that vertices select from in the graph.
        /// </summary>
        private IDictionary<string, Type> Properties { get; set; }

        /// <summary>
        /// Creates a new random sampled, infinite, directed graph with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters to generate the graph with.</param>
        public InfiniteDirectedGraph(InfiniteDirectedGraphParameters parameters)
        {
            // Set the parameters.
            Parameters = parameters;

            // Generate our random properties.
            CompoundRandom random = new CompoundRandom(Parameters.Seed);
            Properties = new Dictionary<string, Type>();
            while (Properties.Count < Parameters.PropertyCount)
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
        /// Randomly generates a vertex by its identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>The vertex with its properties assigned.</returns>
        protected FreeformVertex GetProperties(Guid id)
        {
            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new CompoundRandom(Parameters.Seed, id);

            // Construct a dictionary of random properties.
            List<FreeformProperty> properties = new List<FreeformProperty>();
            foreach (KeyValuePair<string, Type> property in Properties)
            {
                if (random.NextDouble() < Parameters.PropertyDensity)
                    properties.Add(new FreeformProperty(FreeformIdentity.Create(property.Key))
                    {
                        Observations = new List<FreeformObservation>
                        {
                            new FreeformObservation
                            {
                                Type = property.Value.ToString(),
                                Value = GenerateRandomValue(random, property.Value)
                            }
                        }
                    });
            }

            // Return the randomly generated vertex with properties.
            return new FreeformVertex(FreeformIdentity.Create(id))
            {
                Properties = properties
            };
        }
        /// <summary>
        /// Randomly generates an enumerable of out-edges of a vertex by its identifier.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>The enumerable of out-edges of the vertex.</returns>
        protected IEnumerable<FreeformEdge> GetEdges(Guid id)
        {
            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new CompoundRandom(Parameters.Seed, id);

            // Keep constructing children until the random generator doesn't sample within the current probability.
            int child = 0;
            double probability = Parameters.ChildProbability;
            while (random.NextDouble() < probability)
            {
                // We must construct the ID from the random number generator and not from the system.
                Guid randomId = random.NextGuid();

                // The source of each edge is the selected vertex.
                yield return new FreeformEdge
                (
                    FreeformIdentity.Create(id),
                    FreeformIdentity.Create(randomId),
                    FreeformIdentity.Create(child++)
                );

                // We lower the probability by some amount each time.
                probability *= Parameters.ChildDampener;
            }
        }

        #region FreeformGraph
        /// <inheritdoc />
        public override bool IsDirected => true;
        /// <inheritdoc />
        public override bool IsFinite => false;
        /// <inheritdoc />
        public override bool AllowParallelEdges => true;

        /// <inheritdoc />
        public override bool IsVerticesEmpty => false;
        /// <inheritdoc />
        public override bool IsEdgesEmpty => Parameters.ChildProbability > 0;

        /// <inheritdoc />
        public override int VertexCount => throw new NotImplementedException();
        /// <inheritdoc />
        public override int EdgeCount => throw new NotImplementedException();

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> Vertices => throw new NotImplementedException();
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges => throw new NotImplementedException();

        /// <inheritdoc />
        public override bool ContainsEdge(FreeformEdge edge)
        {
            // Get the source vertex and check if it randomly generates the edge.
            if (FreeformIdentity.IsType(edge.Source.Identifier, out Guid sourceId))
                return GetEdges(sourceId).Contains(edge);
            return false;
        }
        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            // This graph contains all vertices.
            return true;
        }
        #endregion
    }
}