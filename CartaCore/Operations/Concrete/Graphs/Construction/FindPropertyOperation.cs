using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The input for the <see cref="FindPropertyOperation"/> operation.
    /// </summary>
    public struct FindPropertyOperationIn
    {
        /// <summary>
        /// The name of the property to find.
        /// </summary>
        [FieldRequired]
        [FieldName("Name")]
        public string Name { get; set; }
        /// <summary>
        /// The graph to search.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="FindPropertyOperation"/> operation.
    /// </summary>
    public struct FindPropertyOperationOut
    {
        /// <summary>
        /// The list of values of the property.
        /// If a vertex is missing the specified property, then null will represent the value.
        /// </summary>
        [FieldName("Values")]
        public IAsyncEnumerable<object> Values { get; set; }
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
        private static async IAsyncEnumerable<object> FindProperties(Graph graph, string name)
        {
            if (graph.Components.TryFind(out IEnumerableComponent<Vertex, IEdge> enumerableGraph))
            {
                await foreach (Vertex vertex in enumerableGraph.GetVertices())
                {
                    vertex.Properties.TryGetValue(name, out IProperty property);
                    yield return property.Value;
                }
            }
            else yield break;
        }

        /// <inheritdoc />
        public override async Task<FindPropertyOperationOut> Perform(FindPropertyOperationIn input)
        {
            return await Task.FromResult(new FindPropertyOperationOut
            {
                Values = FindProperties(input.Graph, input.Name)
            });
        }
    }
}