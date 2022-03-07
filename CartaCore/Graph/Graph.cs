using System.Collections.Generic;
using CartaCore.Graphs.Components;

namespace CartaCore.Graphs
{
    /// <summary>
    /// Represents a graph structure that flexibly retrieves vertex and edge values containing properties with numerous
    /// observations.
    /// </summary>
    public abstract class Graph : Element<Graph>, IGraph
    {
        /// <inheritdoc />
        public abstract GraphAttributes Attributes { get; }

        /// <inheritdoc />
        public ComponentStack Components { get; set; } = new();

        /// <summary>
        /// Initializes an instance of the <see cref="Graph"/> class with its specified identifier and a set of
        /// properties assigned to it.
        /// </summary>
        /// <param name="id">The identifier of this graph.</param>
        /// <param name="properties">The properties assigned to this element.</param>
        protected Graph(string id, IDictionary<string, IProperty> properties)
            : base(id, properties) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Graph"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this graph.</param>
        protected Graph(string id)
            : base(id) { }
    }
}