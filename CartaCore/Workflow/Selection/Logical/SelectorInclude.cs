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
    /// Selects vertices by including vertices with specified internal IDs.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("include")]
    [DiscriminantSemantics(Name = "Include Vertices", Group = "Logical", Hidden = true)]
    public class SelectorInclude : Selector
    {
        #region API Parameters
        /// <summary>
        /// The IDs of vertices to include in the selection.
        /// </summary>
        [DataMember(Name = "ids")]
        [Display(Name = "IDs")]
        [Required]
        public List<string> Ids { get; set; }
        #endregion

        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            return Task.FromResult(Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)));
        }

        /// <inheritdoc />
        public override IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
                return dynamic.GetVertices(Ids.Select(id => Identity.Create(id)));
            else return base.GetVertices();
        }
    }
}