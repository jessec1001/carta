using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph that wraps around another instance of a graph to provide additional functionality.
    /// </summary>
    public abstract class WrapperGraph : Graph
    {
        /// <inheritdoc />
        protected WrapperGraph(string id) : base(id) { }

        /// <inheritdoc />
        protected WrapperGraph(string id, ISet<IProperty> properties) : base(id, properties) { }

        /// <summary>
        /// Gets the wrapped graph.
        /// </summary>
        /// <value>The underlying graph that this graph wraps around.</value>
        protected abstract IGraph WrappedGraph { get; }

        /// <inheritdoc />
        public override GraphAttributes Attributes => WrappedGraph.Attributes;

        /// <summary>
        /// Tries to provide this object as a type implementing the specified functionality.
        /// </summary>
        /// <param name="func">The provided functionality or <c>default(U)</c>.</param>
        /// <typeparam name="U">The type of functionality to provide.</typeparam>
        /// <returns>
        /// <c>true</c> if the functionality could be retrieved and provided successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool TryProvide<U>(out U func) where U : IGraph
        {
            if (this is U fn)
            {
                func = fn;
                return true;
            }
            else if (WrappedGraph.TryProvide(out U wrappedFn))
            {
                func = wrappedFn;
                return true;
            }
            else
            {
                func = default;
                return false;
            }
        }
    }
}