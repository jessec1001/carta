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
    /// Selects vertices by excluding vertices with specified internal IDs.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("exclude")]
    [DiscriminantSemantics(Name = "Exclude Vertices", Group = "Logical", Hidden = true)]
    public class SelectorExclude : Selector
    {
        #region API Parameters
        /// <summary>
        /// The IDs of vertices to exclude in the selection.
        /// </summary>
        [DataMember(Name = "ids")]
        [Display(Name = "IDs")]
        [Required]
        public List<string> Ids { get; set; }
        #endregion

        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            return Task.FromResult(!Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)));
        }
    }
}