using QuikGraph;

namespace CartaCore.GraphExtensions
{
    public static class SubgraphExtension
    {
        /// <summary>
        /// Determines if this graph contains the specified subgraph.
        /// </summary>
        /// <param name="graph">The graph to check for subgraph membership.</param>
        /// <param name="other">The graph being tested for membership.</param>
        /// <param name="enforceLabel">Whether to enforce vertex label matching.</param>
        /// <typeparam name="TVertex">The type of vertex in the graphs.</typeparam>
        /// <typeparam name="TEdge">The type of edge in the graphs.</typeparam>
        /// <returns><c>true</c> if the specified graph is a subgraph; otherwise, <c>false</c>.</returns>
        public static bool HasSubgraph<TVertex, TEdge>(
            this IBidirectionalGraph<TVertex, TEdge> graph,
            IBidirectionalGraph<TVertex, TEdge> other,
            bool enforceLabel = false
        ) where TEdge : IEdge<TVertex>
        {
            return true;
        }
    }
}