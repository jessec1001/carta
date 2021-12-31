using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using MorseCode.ITask;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="ReverseEdgesOperation"/> operation.
    /// </summary>
    public struct ReverseEdgesOperationIn
    {
        /// <summary>
        /// The graph to reverse the edges of.
        /// </summary>
        public Graph Graph { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ReverseEdgesOperation"/> operation.
    /// </summary>
    public struct ReverseEdgesOperationOut
    {
        /// <summary>
        /// The resulting graph containing the reversed edges.
        /// </summary>
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Reverses the edges of a graph.
    /// </summary>
    public class ReverseEdgesOperation : TypedOperation
    <
        ReverseEdgesOperationIn,
        ReverseEdgesOperationOut
    >
    {
        /// <summary>
        /// A graph where the edges are reversed.
        /// </summary>
        private class ReverseEdgesGraph :
            WrapperGraph,
            IDynamicInGraph<Vertex>,
            IDynamicOutGraph<Vertex>,
            IEntireGraph
        {
            /// <summary>
            /// The graph that is wrapped by this filter.
            /// </summary>
            public Graph Graph { get; private init; }

            /// <inheritdoc />
            protected override IGraph WrappedGraph => Graph;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReverseEdgesGraph"/> class.
            /// </summary>
            /// <param name="graph">The graph to reverse edges of.</param>
            public ReverseEdgesGraph(Graph graph)
                : base(graph.Identifier, graph.Properties)
            {
                Graph = graph;
            }

            /// <inheritdoc />
            public override bool TryProvide<U>(out U func)
            {
                if (typeof(U).IsAssignableTo(typeof(IRootedGraph)))
                {
                    func = default;
                    return false;
                }
                else
                {
                    bool success = ((IGraph)Graph).TryProvide(out U wrappedFunc);
                    func = wrappedFunc;
                    return success;
                }
            }

            /// <inheritdoc />
            public async IAsyncEnumerable<Vertex> GetChildVertices(Identity id)
            {
                if (((IGraph)Graph).TryProvide(out IDynamicInGraph<Vertex> dynamicInGraph))
                {
                    await foreach (Vertex parentVertex in dynamicInGraph.GetParentVertices(id))
                        yield return parentVertex;
                }
                else
                {
                    Vertex vertex = await GetVertex(id);
                    foreach (Edge inEdge in vertex.InEdges)
                        yield return await GetVertex(inEdge.Source);
                }
            }
            /// <inheritdoc />
            public async IAsyncEnumerable<Vertex> GetParentVertices(Identity id)
            {
                if (((IGraph)Graph).TryProvide(out IDynamicOutGraph<Vertex> dynamicOutGraph))
                {
                    await foreach (Vertex childVertex in dynamicOutGraph.GetChildVertices(id))
                        yield return childVertex;
                }
                else
                {
                    Vertex vertex = await GetVertex(id);
                    foreach (Edge outEdge in vertex.OutEdges)
                        yield return await GetVertex(outEdge.Target);
                }
            }
            /// <inheritdoc />
            public async ITask<Vertex> GetVertex(Identity id)
            {
                if (((IGraph)Graph).TryProvide(out IDynamicGraph<IVertex> dynamic))
                {
                    IVertex vertex = await dynamic.GetVertex(id);
                    IEnumerable<Edge> edges = vertex.Edges.Select(
                        edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties)
                    );
                    return new Vertex(id, vertex.Properties, edges)
                    {
                        Label = vertex.Label,
                        Description = vertex.Description
                    };
                }
                else throw new NotSupportedException();
            }
            /// <inheritdoc />
            public async IAsyncEnumerable<IVertex> GetVertices()
            {
                if (((IGraph)Graph).TryProvide(out IEntireGraph entire))
                {
                    await foreach (IVertex vertex in entire.GetVertices())
                    {
                        IEnumerable<Edge> edges = vertex.Edges.Select(
                            edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties)
                        );
                        yield return new Vertex(vertex.Identifier, vertex.Properties, edges)
                        {
                            Label = vertex.Label,
                            Description = vertex.Description
                        };
                    }
                }
                else throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public override async Task<ReverseEdgesOperationOut> Perform(ReverseEdgesOperationIn input)
        {
            return await Task.FromResult(
                new ReverseEdgesOperationOut()
                {
                    Graph = new ReverseEdgesGraph(input.Graph)
                }
            );
        }
    }
}