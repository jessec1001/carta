using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects vertices, edges, properties, and values depending on certain criterion specified by implementations of
    /// this class. Also supports iterating over all vertices in a graph.
    /// </summary>
    [JsonSchemaFlatten]
    [DiscriminantBase]
    public abstract class Selector : WrapperGraph,
        IEntireGraph
    {
        /// <summary>
        /// The base graph that this selector graph is being applied to.
        /// </summary>
        [JsonSchemaIgnore]
        public IGraph Graph { get; set; }

        /// <inheritdoc />
        protected override IGraph WrappedGraph => Graph;

        /// <summary>
        /// Determines whether the selector contains the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns><c>true</c> if the selector contains the vertex; otherwise <c>false</c>.</returns>
        public virtual Task<bool> ContainsVertex(IVertex vertex) => Task.FromResult(true);
        /// <summary>
        /// Determines whether the selector contains the specified edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <returns><c>true</c> if the selector contains the edge; otherwise <c>false</c>.</returns>
        public virtual Task<bool> ContainsEdge(Edge edge) => Task.FromResult(true);
        /// <summary>
        /// Determines whether the selector contains the specified property. The property may be on a vertex or edge.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the selector contains the property; otherwise <c>false</c>.</returns>
        public virtual Task<bool> ContainsProperty(Property property) => Task.FromResult(true);
        /// <summary>
        /// Determines whether the selector contains the specified value. The value is from within a property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the selector contains the value; otherwise <c>false</c>.</returns>
        public virtual Task<bool> ContainsValue(object value) => Task.FromResult(true);

        /// <inheritdoc />
        public virtual IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
                return entire.GetVertices().WhereAwait(async vertex => await ContainsVertex(vertex));
            else throw new NotSupportedException();
        }
    }
}