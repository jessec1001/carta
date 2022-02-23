using System;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Implement support for pipelining these values.

    /// <summary>
    /// The input for the <see cref="FindPropertyOperation"/> operation.
    /// </summary>
    public struct FindPropertyOperationIn
    {
        /// <summary>
        /// The name of the property to find.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The graph to search.
        /// </summary>
        public Graph Graph { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="FindPropertyOperation"/> operation.
    /// </summary>
    public struct FindPropertyOperationOut
    {
        /// <summary>
        /// The list of values of the property.
        /// </summary>
        public object[] Values { get; set; }
    }

    /// <summary>
    /// Locates a property on all vertices in a graph. Yields a list of all values of the property.
    /// </summary>
    [OperationName(Display = "Find Graph Property", Type = "findGraphProperty")]
    [OperationTag(OperationTags.Graph)]
    public class FindPropertyOperation : TypedOperation
    <
        FindPropertyOperationIn,
        FindPropertyOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<FindPropertyOperationOut> Perform(FindPropertyOperationIn input)
        {
            // Create an empty list of values
            object[] values;

            // TODO: Improve fetching properties by an identifier.
            // Check if the graph can be enumerated over.
            // If so, grab the values from its vertices.
            IGraph graph = input.Graph;
            if (graph.TryProvide(out IEntireGraph<Vertex, IEdge> entireGraph))
            {
                values = await entireGraph.GetVertices()
                    .Select(vertex => vertex
                        .Properties
                        .FirstOrDefault(property => property.Id == input.Name)
                        .Value
                    )
                    .ToArrayAsync();
            }
            else values = Array.Empty<object>();

            return await Task.FromResult(new FindPropertyOperationOut() { Values = values });
        }
    }
}