using System;
using System.Linq;

using QuikGraph;

using CartaCore.Data;

namespace CartaCore.Workflow.Selection
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Represents a selection of vertices and edges in a freeform graph.
    /// </summary>
    public abstract class SelectorBase
    {
        /// <summary>
        /// Filters the vertices and edges of a graph based on a selection.
        /// </summary>
        /// <param name="graph">The graph containing the vertices and edges to filter.</param>
        /// <returns>The filtered graph.</returns>
        public abstract FreeformGraph Filter(FreeformGraph graph);

        /// <summary>
        /// Filters the vertices and edges of a graph based on a condition.
        /// </summary>
        /// <param name="graph">The graph containing the vertices and edges to filter.</param>
        /// <param name="condition">The condition to filter on.</param>
        /// <returns>The filtered graph.</returns>
        protected FreeformGraph FilterByCondition(FreeformGraph graph, Predicate<FreeformVertex> condition)
        {
            // Create a new graph of the correct directed variant.
            FreeformGraph filteredGraph;
            if (graph.IsDirected)
                filteredGraph = new AdjacencyGraph<FreeformVertex, FreeformEdge>();
            else
                filteredGraph = new UndirectedGraph<FreeformVertex, FreeformEdge>();

            // Add the vertices and edges based on the condition.
            filteredGraph.AddVertexRange
            (
                graph.Vertices.Where(vertex => condition(vertex))
            );
            filteredGraph.AddEdgeRange
            (
                graph.Edges.Where
                (edge =>
                    filteredGraph.ContainsVertex(edge.Source) &&
                    filteredGraph.ContainsVertex(edge.Target)
                )
            );

            return filteredGraph;
        }
    }
}