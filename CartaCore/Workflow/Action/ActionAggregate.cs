using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Action
{
    [DiscriminantDerived("aggregate")]
    public class ActionAggregate : ActionBase
    {
        private void AddVertexProperties(Dictionary<Identity, List<object>> properties, IVertex vertex)
        {
            foreach (Property property in vertex.Properties)
            {
                // Add the property if it does not exist in the set.
                if (!properties.ContainsKey(property.Identifier))
                    properties.Add(property.Identifier, new List<object>());

                // Merge in the set of observations into the property.
                if (properties.TryGetValue(property.Identifier, out List<object> values))
                    values.AddRange(property.Values);
            }
        }

        public async override Task<IVertex> ApplyToVertex(IGraph graph, IVertex vertex)
        {
            if (graph is IDynamicOutGraph<IOutVertex> dynamicOutGraph)
            {
                // Set up data structures to store intermediate information.
                HashSet<Identity> fetchedVertices = new HashSet<Identity>();
                Dictionary<Identity, List<object>> properties = new Dictionary<Identity, List<object>>();

                List<IAsyncEnumerable<IOutVertex>> childrenVertices = new List<IAsyncEnumerable<IOutVertex>>();

                // Kick off the algorithm by grabbing the parents of this vertex.
                AddVertexProperties(properties, vertex);
                fetchedVertices.Add(vertex.Identifier);
                childrenVertices.Add(dynamicOutGraph.GetChildVertices(vertex.Identifier));

                // Keep getting all the parents asynchronously and updating our properties collection.
                while (childrenVertices.Count > 0)
                {
                    // Get the earliest queued parent item.
                    IAsyncEnumerable<IOutVertex> childVertices = childrenVertices.First();
                    childrenVertices.RemoveAt(0);

                    // Iterate over the parent vertex collection propagating properties.
                    await foreach (IInVertex childVertex in childVertices)
                    {
                        // We need to make sure that we do not duplicate observations.
                        if (!fetchedVertices.Contains(childVertex.Identifier))
                        {
                            AddVertexProperties(properties, childVertex);
                            fetchedVertices.Add(childVertex.Identifier);
                            childrenVertices.Add(dynamicOutGraph.GetChildVertices(childVertex.Identifier));
                        }
                    }
                }

                // Set the properties of the vertex.
                return new Vertex
                (
                    vertex.Identifier,
                    properties
                    .Select
                    (
                        (KeyValuePair<Identity, List<object>> pair) =>
                        new Property(pair.Key) { Values = pair.Value }
                    )
                )
                {
                    Label = vertex.Label,
                    Description = vertex.Description,
                };
            }
            return vertex;
        }
    }
}