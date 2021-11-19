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
        IDynamicInGraph<Vertex>,
        IDynamicOutGraph<Vertex>,
        IEntireGraph
    {
        /// <inheritdoc />
        protected override bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IRootedGraph))) return false;
            if (type.IsAssignableTo(typeof(IDynamicInGraph<Vertex>))) return Graph.CanProvide<IDynamicInGraph<Vertex>>();
            if (type.IsAssignableTo(typeof(IDynamicOutGraph<Vertex>))) return Graph.CanProvide<IDynamicOutGraph<Vertex>>();
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            return base.ShouldProvide(type);
        }

        /// <inheritdoc />
        public new async ITask<Vertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IVertex vertex = await dynamic.GetVertex(id);
                IEnumerable<Edge> edges = vertex.Edges.Select(
                    edge => new Edge(edge.Identifier, edge.Target, edge.Source, edge.Properties)
                );
                return new Vertex(vertex.Identifier, vertex.Properties, edges)
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
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetParentVertices(Identity id)
        {
            if (Graph.TryProvide(out IDynamicOutGraph<Vertex> dynamicOutGraph))
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
        public async IAsyncEnumerable<Vertex> GetChildVertices(Identity id)
        {
            if (Graph.TryProvide(out IDynamicInGraph<Vertex> dynamicInGraph))
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
    }
}