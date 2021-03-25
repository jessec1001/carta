using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MorseCode.ITask;

using CartaCore.Statistics;
using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    /// <summary>
    /// Represents graph data of a random, (practically) infinite, directed graph. Both the vertices and edges are
    /// randomly generated and connected.
    /// </summary>
    public class InfiniteDirectedGraph : Graph,
        IDynamicGraph<OutVertex>,
        IDynamicOutGraph<OutVertex>,
        IParameterizedGraph<InfiniteDirectedGraphParameters>
    {
        /// <inheritdoc />
        public InfiniteDirectedGraphParameters Parameters { get; set; }

        /// <summary>
        /// Gets or sets the named types of properties used in the graph.
        /// </summary>
        /// <value>A map of name-type pairs of the names and types of each property included in this graph.</value>
        private IDictionary<string, Type> NamedPropertyTypes { get; set; }

        /// <summary>
        /// Creates a new random sampled, infinite, directed graph with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters to generate the graph with.</param>
        public InfiniteDirectedGraph(InfiniteDirectedGraphParameters parameters)
            : base(Identity.Create(nameof(InfiniteDirectedGraph)))
        {
            // Set the parameters.
            Parameters = parameters;

            // Generate our random properties.
            CompoundRandom random = new CompoundRandom(Parameters.Seed);
            int propertyCount = Parameters.PropertyCount.Sample(random);
            NamedPropertyTypes = new Dictionary<string, Type>();
            while (NamedPropertyTypes.Count < propertyCount)
            {
                string key = random.NextPsuedoword();
                Type type = GenerateRandomType(random);

                if (!NamedPropertyTypes.ContainsKey(key))
                    NamedPropertyTypes.Add(key, type);
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

        /// <inheritdoc />
        public override bool IsDirected => true;
        /// <inheritdoc />
        public override bool IsDynamic => true;
        /// <inheritdoc />
        public override bool IsFinite => false;

        /// <inheritdoc />
        public Identity BaseIdentifier => Identity.Create(new CompoundRandom(Parameters.Seed).NextGuid());

        /// <summary>
        /// Generates a requested vertex based on its identifier and the graph parameters.
        /// </summary>
        /// <param name="id">The vertex identifier.</param>
        /// <returns>The requested vertex generated randomly.</returns>
        protected OutVertex GenerateVertex(Identity id)
        {
            // Check that we can convert the specified ID to a GUID.
            if (!id.IsType(out Guid guid)) throw new InvalidCastException();

            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new CompoundRandom(Parameters.Seed, guid);

            // Create properties from the set of graph properties.
            List<Property> properties = new List<Property>(capacity: NamedPropertyTypes.Count);
            foreach (KeyValuePair<string, Type> namedPropertyType in NamedPropertyTypes)
            {
                if (random.NextDouble() < Parameters.PropertyInclusionProbability)
                {
                    // Add a single observation to the property.
                    List<Observation> observations = new List<Observation>(capacity: 1);
                    observations.Add
                    (
                        new Observation
                        {
                            Type = namedPropertyType.Value.TypeSerialize(),
                            Value = GenerateRandomValue(random, namedPropertyType.Value)
                        }
                    );

                    Property property = new Property(Identity.Create(namedPropertyType.Key), observations);
                    properties.Add(property);
                }
            }
            properties.TrimExcess();

            // Create out-edges.
            int childCount = Parameters.ChildCount.Sample(random);
            List<Edge> outEdges = new List<Edge>(capacity: childCount);
            for (int index = 0; index < childCount; index++)
            {
                // We must construct the ID from the random number generator and not from the system.
                Guid randomId = random.NextGuid();

                // The source of each edge is the selected vertex.
                outEdges.Add(new Edge(id, Identity.Create(randomId)));
            }

            // Return the randomly generated vertex with properties.
            OutVertex vertex = new OutVertex(id, properties, outEdges);
            if (Parameters.Labeled) vertex.Label = random.NextPsuedoword();
            return vertex;
        }

        /// <inheritdoc />
        public ITask<OutVertex> GetVertex(Identity id)
        {
            return Task.FromResult<OutVertex>(GenerateVertex(id)).AsITask();
        }
    }
}