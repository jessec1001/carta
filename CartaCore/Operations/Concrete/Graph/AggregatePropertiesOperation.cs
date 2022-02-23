using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Implement pipelining for this operation.
    // TODO: Fix typing of graph structure.

    /// <summary>
    /// The input for the <see cref="AggregatePropertiesOperation"/> operation.
    /// </summary>
    public struct AggregatePropertiesOperationIn
    {
        /// <summary>
        /// The graph containing the aggregation vertex.
        /// </summary>
        public Graph Graph { get; set; }
        /// <summary>
        /// The vertex to aggregate the properties onto.
        /// </summary>
        public Vertex Vertex { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="AggregatePropertiesOperation"/> operation.
    /// </summary>
    public struct AggregatePropertiesOperationOut
    {
        /// <summary>
        /// The resulting vertex containing the aggregated properties.
        /// </summary>
        public Vertex Vertex { get; set; }
    }

    // TODO: Allow multiplexing a graph onto a vertex field.
    // TODO: This operation will need some reworking to make it more sensible in the new framework.
    /// <summary>
    /// Collects properties of descedants of a vertex onto the ancestor vertex.
    /// </summary>
    [OperationName(Display = "Aggregate Graph Properties", Type = "aggregateProperties")]
    [OperationTag(OperationTags.Graph)]
    public class AggregatePropertiesOperation : TypedOperation
    <
        AggregatePropertiesOperationIn,
        AggregatePropertiesOperationOut
    >
    {
        private static void AddVertexProperties(Dictionary<string, List<object>> properties, Vertex vertex)
        {
            foreach (Property property in vertex.Properties)
            {
                // Add the property if it does not exist in the set.
                if (!properties.ContainsKey(property.Id))
                    properties.Add(property.Id, new List<object>());

                // Merge in the set of observations into the property.
                if (properties.TryGetValue(property.Id, out List<object> values))
                    values.Add(property.Value);
            }
        }

        /// <inheritdoc />
        public override async Task<AggregatePropertiesOperationOut> Perform(AggregatePropertiesOperationIn input)
        {
            if (((IGraph)input.Graph).TryProvide(out IDynamicOutGraph<Vertex, IEdge> dynamicOutGraph))
            {
                // Set up data structures to store intermediate information.
                HashSet<string> fetchedVertices = new();
                Dictionary<string, List<object>> properties = new();

                List<IAsyncEnumerable<Vertex>> childrenVertices = new();

                // Kick off the algorithm by grabbing the children of this vertex.
                AddVertexProperties(properties, input.Vertex);
                fetchedVertices.Add(input.Vertex.Id);
                childrenVertices.Add(dynamicOutGraph.GetChildVertices(input.Vertex.Id));

                // Keep getting all the children asynchronously and updating our properties collection.
                while (childrenVertices.Count > 0)
                {
                    // Get the earliest queued parent item.
                    IAsyncEnumerable<Vertex> childVertices = childrenVertices.First();
                    childrenVertices.RemoveAt(0);

                    // Iterate over the child vertex collection propagating properties.
                    await foreach (Vertex childVertex in childVertices)
                    {
                        // We need to make sure that we do not duplicate observations.
                        if (!fetchedVertices.Contains(childVertex.Id))
                        {
                            AddVertexProperties(properties, childVertex);
                            fetchedVertices.Add(childVertex.Id);
                            childrenVertices.Add(dynamicOutGraph.GetChildVertices(childVertex.Id));
                        }
                    }
                }

                // Set the properties of the vertex.
                return new AggregatePropertiesOperationOut()
                {
                    Vertex = new Vertex
                    (
                        input.Vertex.Id,
                        new HashSet<IProperty>(
                            properties
                            .Select
                            (
                                (KeyValuePair<string, List<object>> pair) =>
                                new Property(pair.Key, pair.Value)
                            )
                        )
                    )
                    {
                        Label = input.Vertex.Label,
                        Description = input.Vertex.Description,
                    }
                };
            }
            else return new AggregatePropertiesOperationOut() { Vertex = input.Vertex };
        }
    }
}