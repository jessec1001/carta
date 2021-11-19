using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MorseCode.ITask;
using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Workflow.Selection;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Acts on selected vertices, edges, properties, and values to perform some calculation or transformation depending
    /// on the individual implementations of this class.
    /// </summary>
    [JsonSchemaFlatten]
    [DiscriminantBase]
    public class Actor : WrapperGraph,
        IEntireGraph,
        IDynamicGraph<IVertex>
    {
        /// <summary>
        /// The base graph that this actor graph is being applied to.
        /// </summary>
        [JsonSchemaIgnore]
        public IGraph Graph { get; set; }
        /// <summary>
        /// The selector graph that determines what subgraph to act on.
        /// </summary>
        [JsonSchemaIgnore]
        public Selector Selector { get; set; }

        /// <inheritdoc />
        protected override IGraph WrappedGraph => Graph;

        /// <summary>
        /// Determines whether this graph can provide the interface for a particular graph type. If it can,
        /// <see cref="TryProvide{U}" /> should return a non-null value when <c>U = type</c>.
        /// </summary>
        /// <param name="type">The graph type in question.</param>
        /// <returns>
        /// <c>true</c> if the graph should provide an interface for the specified type; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            if (type.IsAssignableTo(typeof(IDynamicGraph<IVertex>))) return Graph.CanProvide<IDynamicGraph<IVertex>>();
            return true;
        }
        /// <inheritdoc />
        public override bool TryProvide<U>(out U func)
        {
            bool shouldProvide = ShouldProvide(typeof(U));
            if (!shouldProvide)
            {
                func = default;
                return false;
            }

            bool success = base.TryProvide<U>(out U fn);
            func = fn;
            return success;
        }

        /// <summary>
        /// Transforms a value using the actor.
        /// </summary>
        /// <param name="value">The value to transform.</param>
        /// <returns>The transformed value.</returns>
        public virtual Task<object> TransformValue(object value) => Task.FromResult(value);
        /// <summary>
        /// Transforms a property using the actor.
        /// </summary>
        /// <param name="property">The property to transform.</param>
        /// <returns>The transformed property.</returns>
        public virtual Task<Property> TransformProperty(Property property) => Task.FromResult(property);
        /// <summary>
        /// Transforms an edge using the actor.
        /// </summary>
        /// <param name="edge">The edge to transform.</param>
        /// <returns>The transformed edge.</returns>
        public virtual Task<Edge> TransformEdge(Edge edge) => Task.FromResult(edge);
        /// <summary>
        /// Transforms a vertex using the actor.
        /// </summary>
        /// <param name="vertex">The vertex to transform.</param>
        /// <returns>The transformed vertex.</returns>
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
            return new Property(property.Identifier, await TransformValue(property.Value))
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
            if (vertex is Vertex inVertex)
                inEdges = inVertex.InEdges
                    .Select(async edge => await ReconstructEdge(edge))
                    .Select(task => task.Result);
            if (vertex is Vertex outVertex)
                outEdges = outVertex.OutEdges
                    .Select(async edge => await ReconstructEdge(edge))
                    .Select(task => task.Result);

            inEdges ??= Enumerable.Empty<Edge>();
            outEdges ??= Enumerable.Empty<Edge>();

            IEnumerable<Edge> edges = inEdges.Union(outEdges);
            inEdges = edges.Where(edge => edge.Target.Equals(vertex.Identifier));
            outEdges = edges.Where(edge => edge.Source.Equals(vertex.Identifier));

            vertex = new Vertex
            (
                vertex.Identifier,
                vertex.Properties
                    .Select(async property => await ReconstructProperty(property))
                    .Select(task => task.Result),
                Enumerable.Concat(inEdges, outEdges)
            )
            {
                Label = vertex.Label,
                Description = vertex.Description
            };
            return vertex;
        }

        /// <inheritdoc />
        public async ITask<IVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IVertex vertex = await dynamic.GetVertex(id);
                return await ReconstructVertex(vertex);
            }
            else throw new NotSupportedException();
        }
        /// <inheritdoc />
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