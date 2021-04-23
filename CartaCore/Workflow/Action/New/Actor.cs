using System;
using System.Collections.Generic;
using System.Linq;

using MorseCode.ITask;

using CartaCore.Data;
using CartaCore.Workflow.Selection;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    [DiscriminantBase]
    public class Actor : WrapperGraph,
        IEntireGraph,
        IDynamicGraph<IVertex>
    {
        public IGraph Graph { get; set; }
        public Selector Selector { get; set; }

        protected override IGraph WrappedGraph => Graph;

        protected virtual bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            if (type.IsAssignableTo(typeof(IDynamicGraph<IVertex>))) return Graph.CanProvide<IDynamicGraph<IVertex>>();
            return true;
        }

        public virtual Edge TransformEdge(Edge edge) => edge;
        public virtual IVertex TransformVertex(IVertex vertex) => vertex;

        private IVertex ReconstructVertex(IVertex vertex)
        {
            vertex = TransformVertex(vertex);
            IEnumerable<Edge> inEdges = null;
            IEnumerable<Edge> outEdges = null;
            if (vertex is IInVertex inVertex)
                inEdges = inVertex.InEdges.Select(edge => TransformEdge(edge));
            if (vertex is IOutVertex outVertex)
                outEdges = outVertex.OutEdges.Select(edge => TransformEdge(edge));

            inEdges ??= Enumerable.Empty<Edge>();
            outEdges ??= Enumerable.Empty<Edge>();

            IEnumerable<Edge> edges = inEdges.Union(outEdges);
            inEdges = edges.Where(edge => edge.Target.Equals(vertex.Identifier));
            outEdges = edges.Where(edge => edge.Source.Equals(vertex.Identifier));

            vertex = new InOutVertex(vertex.Identifier, vertex.Properties, inEdges, outEdges)
            {
                Label = vertex.Label,
                Description = vertex.Description
            };
            return vertex;
        }

        public override bool TryProvide<U>(out U func)
        {
            bool shouldProvide = ShouldProvide(typeof(U));
            if (!shouldProvide)
            {
                func = default(U);
                return false;
            }

            bool success = base.TryProvide<U>(out U fn);
            func = fn;
            return success;
        }

        // From IDynamicGraph
        public async ITask<IVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IVertex vertex = await dynamic.GetVertex(id);
                if (await Selector.ContainsVertex(vertex))
                    return ReconstructVertex(vertex);
                else return vertex;
            }
            else throw new NotSupportedException();
        }
        // From IEntireGraph
        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
            {
                await foreach (IVertex vertex in entire.GetVertices())
                {
                    if (await Selector.ContainsVertex(vertex))
                        yield return ReconstructVertex(vertex);
                    else yield return vertex;
                }
            }
            else throw new NotSupportedException();
        }
    }
}