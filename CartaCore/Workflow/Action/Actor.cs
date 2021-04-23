using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public virtual Task<object> TransformValue(object value) => Task.FromResult(value);
        public virtual Task<Property> TransformProperty(Property property) => Task.FromResult(property);
        public virtual Task<Edge> TransformEdge(Edge edge) => Task.FromResult(edge);
        public virtual Task<IVertex> TransformVertex(IVertex vertex) => Task.FromResult(vertex);

        private async Task<object> ReconstructValue(object value)
        {
            if (!await Selector.ContainsValue(value))
                return value;

            return await TransformValue(value);
        }
        private async Task<Property> ReconstructProperty(Property property)
        {
            if (!await Selector.ContainsProperty(property))
                return property;

            property = await TransformProperty(property);
            return new Property
            (
                property.Identifier,
                property.Values.Select(value => TransformValue(value))
            )
            { Subproperties = property.Subproperties };
        }
        private async Task<Edge> ReconstructEdge(Edge edge)
        {
            if (!await Selector.ContainsEdge(edge))
                return edge;

            return await TransformEdge(edge);
        }
        private async Task<IVertex> ReconstructVertex(IVertex vertex)
        {
            if (!await Selector.ContainsVertex(vertex))
                return vertex;

            vertex = await TransformVertex(vertex);
            IEnumerable<Edge> inEdges = null;
            IEnumerable<Edge> outEdges = null;
            if (vertex is IInVertex inVertex)
                inEdges = inVertex.InEdges
                    .Select(async edge => await ReconstructEdge(edge))
                    .Select(task => task.Result);
            if (vertex is IOutVertex outVertex)
                outEdges = outVertex.OutEdges
                    .Select(async edge => await ReconstructEdge(edge))
                    .Select(task => task.Result);

            inEdges ??= Enumerable.Empty<Edge>();
            outEdges ??= Enumerable.Empty<Edge>();

            IEnumerable<Edge> edges = inEdges.Union(outEdges);
            inEdges = edges.Where(edge => edge.Target.Equals(vertex.Identifier));
            outEdges = edges.Where(edge => edge.Source.Equals(vertex.Identifier));

            vertex = new InOutVertex
            (
                vertex.Identifier,
                vertex.Properties
                    .Select(async property => await ReconstructProperty(property))
                    .Select(task => task.Result),
                inEdges,
                outEdges
            )
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
                return await ReconstructVertex(vertex);
            }
            else throw new NotSupportedException();
        }
        // From IEntireGraph
        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
            {
                await foreach (IVertex vertex in entire.GetVertices())
                    yield return await ReconstructVertex(vertex);
            }
            else throw new NotSupportedException();
        }
    }
}