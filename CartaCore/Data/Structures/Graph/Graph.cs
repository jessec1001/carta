using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a graph structure that flexibly retrieves vertex and edge values containing properties with numerous
    /// observations.
    /// </summary>
    public abstract class Graph : Element<Graph>, IGraph
    {
        /// <inheritdoc />
        public abstract bool IsDirected { get; }
        /// <inheritdoc />
        public abstract bool IsDynamic { get; }
        /// <inheritdoc />
        public abstract bool IsFinite { get; }

        /// <summary>
        /// Initializes an instance of the <see cref="Graph"/> class with its specified identifier and a set of
        /// properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this graph.</param>
        /// <param name="properties">The properties assigned to this element.</param>
        protected Graph(Identity id, IEnumerable<Property> properties)
            : base(id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Graph"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this graph.</param>
        protected Graph(Identity id)
            : base(id) { }
    }
}