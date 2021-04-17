using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a logical AND of other selections.
    /// </summary>
    [DiscriminantDerived("and")]
    public class SelectorAnd : Selector
    {
        /// <summary>
        /// Gets or sets the list of selectors to AND together.
        /// </summary>
        /// <returns>The list of selectors that are combined with a logical AND operator.</returns>
        public List<Selector> Selectors { get; set; } = new List<Selector>();

        /// <inheritdoc />
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Selectors is null || Selectors.Count == 0) return true;
            return await Selectors
                .ToAsyncEnumerable()
                .AllAwaitAsync(async selector => await selector.ContainsVertex(vertex));
        }
        public override async Task<bool> ContainsProperty(Property property)
        {
            if (Selectors is null || Selectors.Count == 0) return true;
            return await Selectors
                .ToAsyncEnumerable()
                .AllAwaitAsync(async selector => await selector.ContainsProperty(property));
        }
        public override async Task<bool> ContainsValue(object value)
        {
            if (Selectors is null || Selectors.Count == 0) return true;
            return await Selectors
                .ToAsyncEnumerable()
                .AllAwaitAsync(async selector => await selector.ContainsValue(value));
        }

        public override async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Selectors is null || Selectors.Count == 0)
            {
                await foreach (IVertex vertex in base.GetVertices())
                    yield return vertex;
            }

            HashSet<IVertex> vertices = null;
            foreach (Selector selector in Selectors)
            {
                if (vertices is null)
                    vertices = new HashSet<IVertex>(await selector.GetVertices().ToArrayAsync());
                else
                    vertices.IntersectWith(await selector.GetVertices().ToListAsync());
            }
            foreach (IVertex vertex in vertices)
                yield return vertex;
        }
    }
}