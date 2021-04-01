using System;
using System.Linq;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of vertices based on a specific property being within a specified range.
    /// </summary>
    [DiscriminantDerived("property range")]
    public class SelectorPropertyRange : SelectorBase
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>The name of the property to select by a range.</value>
        public string Property { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the minimum value in the range.
        /// </summary>
        /// <value>The minimum value in the range. If not specified, equivalent to negative infinity.</value>
        public double? Minimum { get; set; }
        /// <summary>
        /// Gets or sets the maximum value in the range.
        /// </summary>
        /// <value>The maximum value in the range. If not specified, equivalent to positive infinity.</value>
        public double? Maximum { get; set; }

        public override bool ContainsProperty(Property property)
        {
            return property.Identifier.Equals(Identity.Create(Property));
        }
        public override bool ContainsValue(object value)
        {
            double number;
            try
            {
                number = Convert.ToDouble(value);
            }
            catch (Exception) { return false; }

            if (Minimum.HasValue && number < Minimum.Value) return false;
            if (Maximum.HasValue && number > Maximum.Value) return false;
            return true;
        }
    }
}