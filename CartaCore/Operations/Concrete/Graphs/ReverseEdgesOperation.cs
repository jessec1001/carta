// TODO: Reimplement again later.
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using CartaCore.Data;
// using CartaCore.Operations.Attributes;
// using MorseCode.ITask;

// namespace CartaCore.Operations
// {
//     // TODO: Implement pipelining for this operation.
//     // TODO: Fix typing of graph structure.

//     /// <summary>
//     /// The input for the <see cref="ReverseEdgesOperation"/> operation.
//     /// </summary>
//     public struct ReverseEdgesOperationIn
//     {
//         /// <summary>
//         /// The graph to reverse the edges of.
//         /// </summary>
//         public Graph Graph { get; set; }
//     }
//     /// <summary>
//     /// The output for the <see cref="ReverseEdgesOperation"/> operation.
//     /// </summary>
//     public struct ReverseEdgesOperationOut
//     {
//         /// <summary>
//         /// The resulting graph containing the reversed edges.
//         /// </summary>
//         public Graph Graph { get; set; }
//     }

//     /// <summary>
//     /// Reverses the edges of a graph.
//     /// </summary>
//     [OperationName(Display = "Reverse Graph Edges", Type = "reverseEdges")]
//     [OperationTag(OperationTags.Graph)]
//     public class ReverseEdgesOperation : TypedOperation
//     <
//         ReverseEdgesOperationIn,
//         ReverseEdgesOperationOut
//     >
//     {
//         /// <summary>
//         /// A graph where the edges are reversed.
//         /// </summary>
//         private class ReverseEdgesGraph :
//             WrapperGraph,
//             IDynamicInGraph<Vertex, Edge>,
//             IDynamicOutGraph<Vertex, Edge>,
//             IEntireGraph<Vertex, Edge>
//         {
//             /// <summary>
//             /// The graph that is wrapped by this filter.
//             /// </summary>
//             public Graph Graph { get; private init; }

//             /// <inheritdoc />
//             protected override IGraph WrappedGraph => Graph;

//             /// <summary>
//             /// Initializes a new instance of the <see cref="ReverseEdgesGraph"/> class.
//             /// </summary>
//             /// <param name="graph">The graph to reverse edges of.</param>
//             public ReverseEdgesGraph(Graph graph)
//                 : base(graph.Id, graph.Properties)
//             {
//                 Graph = graph;
//             }

//             /// <inheritdoc />
//             public override bool TryProvide<U>(out U func)
//             {
//                 // TODO: Provide correct provisions for swapping dynamic in and out functionalities.
//                 if (typeof(U).IsAssignableTo(typeof(IRootedGraph)))
//                 {
//                     func = default;
//                     return false;
//                 }
//                 else
//                 {
//                     bool success = ((IGraph)Graph).TryProvide(out U wrappedFunc);
//                     func = wrappedFunc;
//                     return success;
//                 }
//             }

//             /// <inheritdoc />
//             public async IAsyncEnumerable<Vertex> GetChildVertices(string id)
//             {
//                 if (((IGraph)Graph).TryProvide(out IDynamicInGraph<Vertex, Edge> dynamicInGraph))
//                 {
//                     await foreach (Vertex parentVertex in dynamicInGraph.GetParentVertices(id))
//                         yield return parentVertex;
//                 }
//                 else
//                 {
//                     Vertex vertex = await GetVertex(id);
//                     foreach (Edge inEdge in vertex.InEdges)
//                         yield return await GetVertex(inEdge.Source);
//                 }
//             }
//             /// <inheritdoc />
//             public async IAsyncEnumerable<Vertex> GetParentVertices(string id)
//             {
//                 if (((IGraph)Graph).TryProvide(out IDynamicOutGraph<Vertex, Edge> dynamicOutGraph))
//                 {
//                     await foreach (Vertex childVertex in dynamicOutGraph.GetChildVertices(id))
//                         yield return childVertex;
//                 }
//                 else
//                 {
//                     Vertex vertex = await GetVertex(id);
//                     foreach (Edge outEdge in vertex.OutEdges)
//                         yield return await GetVertex(outEdge.Target);
//                 }
//             }
//             /// <inheritdoc />
//             public async ITask<Vertex> GetVertex(string id)
//             {
//                 if (((IGraph)Graph).TryProvide(out IDynamicGraph<Vertex, Edge> dynamic))
//                 {
//                     Vertex vertex = await dynamic.GetVertex(id);
//                     IEnumerable<Edge> edges = vertex.Edges.Select(
//                         edge => new Edge(edge.Id, edge.Target, edge.Source, edge.Properties)
//                     );
//                     return new Vertex(id, vertex.Properties, edges)
//                     {
//                         Label = vertex.Label,
//                         Description = vertex.Description
//                     };
//                 }
//                 else throw new NotSupportedException();
//             }
//             /// <inheritdoc />
//             public async IAsyncEnumerable<Vertex> GetVertices()
//             {
//                 if (((IGraph)Graph).TryProvide(out IEntireGraph<Vertex, Edge> entire))
//                 {
//                     await foreach (Vertex vertex in entire.GetVertices())
//                     {
//                         IEnumerable<Edge> edges = vertex.Edges.Select(
//                             edge => new Edge(edge.Id, edge.Target, edge.Source, edge.Properties)
//                         );
//                         yield return new Vertex(vertex.Id, vertex.Properties, edges)
//                         {
//                             Label = vertex.Label,
//                             Description = vertex.Description
//                         };
//                     }
//                 }
//                 else throw new NotSupportedException();
//             }
//         }

//         /// <inheritdoc />
//         public override async Task<ReverseEdgesOperationOut> Perform(ReverseEdgesOperationIn input)
//         {
//             return await Task.FromResult(
//                 new ReverseEdgesOperationOut()
//                 {
//                     Graph = new ReverseEdgesGraph(input.Graph)
//                 }
//             );
//         }
//     }
// }