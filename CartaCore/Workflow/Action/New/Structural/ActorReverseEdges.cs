using System;
using System.Collections.Generic;
using System.Linq;

using MorseCode.ITask;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    [DiscriminantDerived("reverseEdges")]
    public class ActorReverseEdgesGraph : Actor,
        IDynamicInGraph<InOutVertex>,
        IDynamicOutGraph<InOutVertex>,
        IEntireGraph
    {
        protected override bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IRootedGraph))) return false;
            if (type.IsAssignableTo(typeof(IDynamicInGraph<IInVertex>))) return Graph.CanProvide<IDynamicInGraph<IInVertex>>();
            if (type.IsAssignableTo(typeof(IDynamicOutGraph<IOutVertex>))) return Graph.CanProvide<IDynamicOutGraph<IOutVertex>>();
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            return base.ShouldProvide(type);
        }

        public async ITask<InOutVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IEnumerable<Edge> inEdges = Enumerable.Empty<Edge>();
                IEnumerable<Edge> outEdges = Enumerable.Empty<Edge>();
                IVertex vertex = await dynamic.GetVertex(id);
                if (vertex is IInVertex inVertex)
                    outEdges = inVertex.InEdges.Select(edge => new Edge(edge.Target, edge.Source, edge.Properties));
                if (vertex is IOutVertex outVertex)
                    inEdges = outVertex.OutEdges.Select(edge => new Edge(edge.Target, edge.Source, edge.Properties));
                return new InOutVertex(vertex.Identifier, vertex.Properties, inEdges, outEdges)
                {
                    Label = vertex.Label,
                    Description = vertex.Description
                };
            }
            else throw new NotSupportedException();
        }
        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
            {
                await foreach (IVertex vertex in entire.GetVertices())
                {
                    IEnumerable<Edge> inEdges = Enumerable.Empty<Edge>();
                    IEnumerable<Edge> outEdges = Enumerable.Empty<Edge>();
                    if (vertex is IInVertex inVertex)
                        outEdges = inVertex.InEdges.Select(edge => new Edge(edge.Target, edge.Source, edge.Properties));
                    if (vertex is IOutVertex outVertex)
                        inEdges = outVertex.OutEdges.Select(edge => new Edge(edge.Target, edge.Source, edge.Properties));
                    yield return new InOutVertex(vertex.Identifier, vertex.Properties, inEdges, outEdges)
                    {
                        Label = vertex.Label,
                        Description = vertex.Description
                    };
                }
            }
        }
    }
}