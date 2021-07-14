using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using MorseCode.ITask;
using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Reverses all edges connected to the selected vertices.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("reverseEdges")]
    [DiscriminantSemantics(Name = "Reverse Edges", Group = "Structural")]

    public class ActorReverseEdgesGraph : Actor,
        IDynamicInGraph<InOutVertex>,
        IDynamicOutGraph<InOutVertex>,
        IEntireGraph
    {
        /// <inheritdoc />
        protected override bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IRootedGraph))) return false;
            if (type.IsAssignableTo(typeof(IDynamicInGraph<IInVertex>))) return Graph.CanProvide<IDynamicInGraph<IInVertex>>();
            if (type.IsAssignableTo(typeof(IDynamicOutGraph<IOutVertex>))) return Graph.CanProvide<IDynamicOutGraph<IOutVertex>>();
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            return base.ShouldProvide(type);
        }

        /// <inheritdoc />
        public new async ITask<InOutVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IEnumerable<Edge> inEdges = Enumerable.Empty<Edge>();
                IEnumerable<Edge> outEdges = Enumerable.Empty<Edge>();
                IVertex vertex = await dynamic.GetVertex(id);
                if (vertex is IInVertex inVertex)
                    outEdges = inVertex.InEdges.Select(edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties));
                if (vertex is IOutVertex outVertex)
                    inEdges = outVertex.OutEdges.Select(edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties));
                return new InOutVertex(vertex.Identifier, vertex.Properties, inEdges, outEdges)
                {
                    Label = vertex.Label,
                    Description = vertex.Description
                };
            }
            else throw new NotSupportedException();
        }
        /// <inheritdoc />
        public new async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
            {
                await foreach (IVertex vertex in entire.GetVertices())
                {
                    IEnumerable<Edge> inEdges = Enumerable.Empty<Edge>();
                    IEnumerable<Edge> outEdges = Enumerable.Empty<Edge>();
                    if (vertex is IInVertex inVertex)
                        outEdges = inVertex.InEdges.Select(edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties));
                    if (vertex is IOutVertex outVertex)
                        inEdges = outVertex.OutEdges.Select(edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties));
                    yield return new InOutVertex(vertex.Identifier, vertex.Properties, inEdges, outEdges)
                    {
                        Label = vertex.Label,
                        Description = vertex.Description
                    };
                }
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetParentVertices(Identity id)
        {
            if (Graph.TryProvide(out IDynamicOutGraph<InOutVertex> dynamicOutGraph))
            {
                await foreach (InOutVertex childVertex in dynamicOutGraph.GetChildVertices(id))
                    yield return childVertex;
            }
            else
            {
                InOutVertex vertex = await GetVertex(id);
                foreach (Edge outEdge in vertex.OutEdges)
                    yield return await GetVertex(outEdge.Target);
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetChildVertices(Identity id)
        {
            if (Graph.TryProvide(out IDynamicInGraph<InOutVertex> dynamicInGraph))
            {
                await foreach (InOutVertex parentVertex in dynamicInGraph.GetParentVertices(id))
                    yield return parentVertex;
            }
            else
            {
                InOutVertex vertex = await GetVertex(id);
                foreach (Edge inEdge in vertex.InEdges)
                    yield return await GetVertex(inEdge.Source);
            }
        }
    }
}