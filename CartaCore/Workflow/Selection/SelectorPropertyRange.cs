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
        public string Property { get; set; }
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

        /// <inheritdoc />
        public override bool Contains(IVertex vertex)
        {
            foreach (Property property in vertex.Properties)
            {
                if (property.Identifier.Equals(Identity.Create(Property)))
                    return property.Observations.All
                    (
                        observation =>
                        {
                            if (observation.Value is double value)
                            {
                                if (Minimum.HasValue && value < Minimum.Value) return false;
                                if (Maximum.HasValue && value > Maximum.Value) return false;
                                return true;
                            }
                            else return false;
                        }
                    );
            }
            return false;
        }
    }
}