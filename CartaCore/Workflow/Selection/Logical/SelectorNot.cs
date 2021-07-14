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
    /// Selects the inverse of another selection.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("not")]
    [DiscriminantSemantics(Name = "Not", Group = "Logical", Hidden = true)]
    public class SelectorNot : Selector
    {
        /// <summary>
        /// The selection to invert.
        /// </summary>
        [DataMember(Name = "selector")]
        [Display(Name = "Selector")]
        [Required]
        public Selector Selector { get; set; } = null;

        /// <inheritdoc />
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            return !await Selector.ContainsVertex(vertex);
        }
        /// <inheritdoc />
        public override async Task<bool> ContainsEdge(Edge edge)
        {
            return !await Selector.ContainsEdge(edge);
        }
        // TODO: Rework to allow property selections to be inverted based on higher-level selections.
        // /// <inheritdoc />
        // public override async Task<bool> ContainsProperty(Property property)
        // {
        //     return !await Selector.ContainsProperty(property);
        // }
        // /// <inheritdoc />
        // public override async Task<bool> ContainsValue(object value)
        // {
        //     return !await Selector.ContainsValue(value);
        // }
    }
}