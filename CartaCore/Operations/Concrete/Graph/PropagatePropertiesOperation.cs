using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="PropagatePropertiesOperation"/> operation.
    /// </summary>
    public struct PropagatePropertiesOperationIn
    {
        /// <summary>
        /// The graph containing the propagation vertex.
        /// </summary>
        public Graph Graph { get; set; }
        /// <summary>
        /// The vertex to propagate the properties into.
        /// </summary>
        public Vertex Vertex { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PropagatePropertiesOperation"/> operation.
    /// </summary>
    public struct PropagatePropertiesOperationOut
    {
        /// <summary>
        /// The resulting vertex containing the propagated properties.
        /// </summary>
        public Vertex Vertex { get; set; }
    }

    // TODO: Allow multiplexing a graph onto a vertex field.
    // TODO: This operation will need some reworking to make it more sensible in the new framework.
    /// <summary>
    /// Collects properties of ancestors of a vertex onto the descendant vertex.
    /// </summary>
    [OperationName(Display = "Propagate Graph Properties", Type = "propagateProperties")]
    [OperationTag(OperationTags.Graph)]
    public class PropagatePropertiesOperation : TypedOperation
    <
        PropagatePropertiesOperationIn,
        PropagatePropertiesOperationOut
    >
    {
        private static void AddVertexProperties(Dictionary<Identity, List<object>> properties, IVertex vertex)
        {
            foreach (Property property in vertex.Properties)
            {
                // Add the property if it does not exist in the set.
                if (!properties.ContainsKey(property.Identifier))
                    properties.Add(property.Identifier, new List<object>());

                // Merge in the set of observations into the property.
                if (properties.TryGetValue(property.Identifier, out List<object> values))
                    values.Add(property.Value);
            }
        }

        /// <inheritdoc />
        public override async Task<PropagatePropertiesOperationOut> Perform(PropagatePropertiesOperationIn input)
        {
            if (((IGraph)input.Graph).TryProvide(out IDynamicInGraph<Vertex> dynamicInGraph))
            {
                // Set up data structures to store intermediate information.
                HashSet<Identity> fetchedVertices = new();
                Dictionary<Identity, List<object>> properties = new();

                List<IAsyncEnumerable<Vertex>> parentsVertices = new();

                // Kick off the algorithm by grabbing the parents of this vertex.
                AddVertexProperties(properties, input.Vertex);
                fetchedVertices.Add(input.Vertex.Identifier);
                parentsVertices.Add(dynamicInGraph.GetParentVertices(input.Vertex.Identifier));

                // Keep getting all the parents asynchronously and updating our properties collection.
                while (parentsVertices.Count > 0)
                {
                    // Get the earliest queued parent item.
                    IAsyncEnumerable<Vertex> parentVertices = parentsVertices.First();
                    parentsVertices.RemoveAt(0);

                    // Iterate over the parent vertex collection propagating properties.
                    await foreach (Vertex parentVertex in parentVertices)
                    {
                        // We need to make sure that we do not duplicate observations.
                        if (!fetchedVertices.Contains(parentVertex.Identifier))
                        {
                            AddVertexProperties(properties, parentVertex);
                            fetchedVertices.Add(parentVertex.Identifier);
                            parentsVertices.Add(dynamicInGraph.GetParentVertices(parentVertex.Identifier));
                        }
                    }
                }

                // Set the properties of the vertex.
                return new PropagatePropertiesOperationOut()
                {
                    Vertex = new Vertex
                    (
                        input.Vertex.Identifier,
                        properties
                        .Select
                        (
                            (KeyValuePair<Identity, List<object>> pair) =>
                            new Property(pair.Key, pair.Value)
                        )
                    )
                    {
                        Label = input.Vertex.Label,
                        Description = input.Vertex.Description,
                    }
                };
            }
            else return new PropagatePropertiesOperationOut() { Vertex = input.Vertex };
        }
    }
}