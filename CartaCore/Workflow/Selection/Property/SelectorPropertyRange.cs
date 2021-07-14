using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects values of a specific property within a specified range.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("propertyRange")]
    [DiscriminantSemantics(Name = "Select Property Range", Group = "Property")]
    public class SelectorPropertyRange : Selector
    {
        /// <summary>
        /// The name of the property containing values to select.
        /// </summary>
        [DataMember(Name = "propertyName")]
        [Display(Name = "Property Name")]
        [Required]
        public string Property { get; set; } = string.Empty;
        /// <summary>
        /// The minimum value in the range. If set to null, equivalent to negative infinity.
        /// </summary>
        [DataMember(Name = "minimum")]
        [Display(Name = "Minimum Value")]
        public double? Minimum { get; set; }
        /// <summary>
        /// The maximum value in the range. If set to null, equivalent to positive infinity.
        /// </summary>
        [DataMember(Name = "maximum")]
        [Display(Name = "Maximum Value")]
        public double? Maximum { get; set; }

        /// <inheritdoc />
        public override Task<bool> ContainsProperty(Property property)
        {
            return Task.FromResult(property.Identifier.Equals(Identity.Create(Property)));
        }
        /// <inheritdoc />
        public override Task<bool> ContainsValue(object value)
        {
            double number;
            try
            {
                number = Convert.ToDouble(value);
            }
            catch (Exception) { return Task.FromResult(false); }

            if (Minimum.HasValue && number < Minimum.Value) return Task.FromResult(false);
            if (Maximum.HasValue && number > Maximum.Value) return Task.FromResult(false);
            return Task.FromResult(true);
        }
    }
}