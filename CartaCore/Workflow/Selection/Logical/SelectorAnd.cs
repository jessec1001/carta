using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

using NJsonSchema.Annotations;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects vertices, properties, and values based on a logical AND of other selections.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("and")]
    [DiscriminantSemantics(Name = "And", Group = "Logical", Hidden = true)]
    public class SelectorAnd : Selector
    {
        /// <summary>
        /// The list of selectors that are combined with a logical AND operator.
        /// </summary>
        [DataMember(Name = "selectors")]
        [Display(Name = "Selectors")]
        [Required]
        public List<Selector> Selectors { get; set; } = new List<Selector>();

        /// <inheritdoc />
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Selectors is null || Selectors.Count == 0) return true;
            return await Selectors
                .ToAsyncEnumerable()
                .AllAwaitAsync(async selector => await selector.ContainsVertex(vertex));
        }
        /// <inheritdoc />
        public override async Task<bool> ContainsProperty(Property property)
        {
            if (Selectors is null || Selectors.Count == 0) return true;
            return await Selectors
                .ToAsyncEnumerable()
                .AllAwaitAsync(async selector => await selector.ContainsProperty(property));
        }
        /// <inheritdoc />
        public override async Task<bool> ContainsValue(object value)
        {
            if (Selectors is null || Selectors.Count == 0) return true;
            return await Selectors
                .ToAsyncEnumerable()
                .AllAwaitAsync(async selector => await selector.ContainsValue(value));
        }

        /// <inheritdoc />
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