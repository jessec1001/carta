using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;

namespace CartaCore.Operations
{
    /// <summary>
    /// A graph that wraps another graph and filters the vertices.
    /// </summary>
    public class FilterGraph : WrapperGraph, IEntireGraph
    {
        /// <summary>
        /// The graph that is wrapped by this filter.
        /// </summary>
        public Graph Graph { get; private init; }
        /// <summary>
        /// The filter used to determine included vertices.
        /// </summary>
        protected Func<IVertex, Task<bool>> Filter { get; private init; }

        /// <inheritdoc />
        protected override IGraph WrappedGraph => Graph;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterGraph"/> class.
        /// </summary>
        /// <param name="graph">The graph to filter.</param>
        /// <param name="vertexFilter">A filter to use to select included vertices.</param>
        public FilterGraph(Graph graph, Func<IVertex, Task<bool>> vertexFilter)
            : base(graph.Identifier, graph.Properties) 
        {
            Filter = vertexFilter;
        }

        /// <inheritdoc />
        public virtual IAsyncEnumerable<IVertex> GetVertices()
        {
            if (((IGraph)Graph).TryProvide(out IEntireGraph entireGraph))
            {
                // TODO: Make sure that edges are filtered as well.
                return entireGraph
                    .GetVertices()
                    .WhereAwait(async vertex => await Filter(vertex));
            }
            else throw new NotSupportedException();
        }
    }
}