using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="PathGraphOperation" /> operation.
    /// </summary>
    public struct PathGraphOperationIn
    {
        /// <summary>
        /// The number of vertices to use in generating the path graph.
        /// </summary>
        public int VertexCount { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PathGraphOperation" /> operation.
    /// </summary>
    public struct PathGraphOperationOut
    {
        /// <summary>
        /// The generated path graph.
        /// </summary>
        public FiniteGraph Graph { get; set; }
    }

    /// <summary>
    /// Generates an undirected path graph of the specified number of vertices. The path graph contains edges between
    /// vertices completing a non-cyclic path that touches each vertex exactly once.
    /// </summary>
    [OperationName(Display = "Generate Path Graph", Type = "generatePathGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Synthetic)]
    public class PathGraphOperation : TypedOperation
    <
        PathGraphOperationIn,
        PathGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<PathGraphOperationOut> Perform(PathGraphOperationIn input)
        {
            // Create the graph structure.
            FiniteGraph graph = new(Identity.Create($"{nameof(PathGraphOperation)}:{input.VertexCount}"), false);

            // Generate the vertices of the graph.
            for (int k = 0; k < input.VertexCount; k++)
            {
                // Create edges between the vertices.
                // Notice that we do not create edges between the first and last vertex.
                List<Edge> edges = new(capacity: 2);
                if (k > 0)
                    edges.Add(new(Identity.Create(k - 1), Identity.Create(k)));
                if (k < input.VertexCount - 1)
                    edges.Add(new(Identity.Create(k), Identity.Create(k + 1)));
                Vertex vertex = new(Identity.Create(k), edges.ToArray());

                // Add the vertex to the graph.
                graph.AddVertex(vertex);
            }

            return Task.FromResult(new PathGraphOperationOut { Graph = graph });
        }
    }
}