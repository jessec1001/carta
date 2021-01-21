using System;
using System.Collections.Generic;
using System.Linq;

using QuikGraph;

using CartaCore.Utility;

namespace CartaCore.Data.Synthetic
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    public class RandomInfiniteDirectedGraph
    {
        private ulong Seed { get; set; }
        private IDictionary<string, Type> Properties { get; set; }
        private double PropertyDensity { get; set; }
        private double ChildProbability { get; set; }
        private double ChildDampener { get; set; }

        public RandomInfiniteDirectedGraph(
            ulong seed = 0,
            int propertyCount = 20,
            double propertyDensity = 0.75,
            double childProbability = 0.80,
            double childDampener = 0.50
        )
        {
            CompoundRandom random = new CompoundRandom(seed);

            Seed = seed;
            Properties = Enumerable
                    .Range(0, propertyCount)
                    .ToDictionary(
                        _ => random.NextPsuedoword(),
                        _ => GenerateRandomType(random)
                    );
            PropertyDensity = propertyDensity;
            ChildProbability = childProbability;
            ChildDampener = childDampener;
        }

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
        protected object GenerateRandomValue(CompoundRandom random, Type type)
        {
            if (typeof(int) == type)
                return random.NextInt(0, 10) + 1;
            if (typeof(double) == type)
                return random.NextDouble();
            if (typeof(string) == type)
                return random.NextPsuedoword();

            // If no types match, return null.
            // This should never happen.
            return null;
        }

        public bool IsFinite() => false;
        public FreeformVertex GetVertexProperties(Guid id)
        {
            // Create a compound random number generator using the GUID and original seed as a combined seed.
            CompoundRandom random = new CompoundRandom(Seed, id);

            // Construct a dictionary of random properties.
            SortedList<string, FreeformVertexProperty> properties = new SortedList<string, FreeformVertexProperty>();
            foreach (KeyValuePair<string, Type> property in Properties)
            {
                if (random.NextDouble() < PropertyDensity)
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
        public IEnumerable<Edge<FreeformVertex>> GetVertexEdges(Guid id)
        {
            CompoundRandom random = new CompoundRandom(Seed, id);

            double probability = ChildProbability;
            while (random.NextDouble() < probability)
            {
                Guid randomId = random.NextGuid();

                yield return new Edge<FreeformVertex>(
                    new FreeformVertex { Id = id },
                    new FreeformVertex { Id = randomId }
                );

                probability *= ChildDampener;
            }
        }
        public FreeformGraph GetGraph() => throw new NotFiniteNumberException();
    }
}