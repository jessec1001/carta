using System;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that specifies the valid range of values for a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldRangeAttribute : Attribute, ISchemaModifierAttribute
    {
        /// <summary>
        /// The minimum value for the field.
        /// If <c>null</c>, there is no lower bound specified for the field.
        /// </summary>
        public double Minimum { get; init; } = double.NegativeInfinity;
        /// <summary>
        /// The maximum value for the field.
        /// If <c>null</c>, there is no upper bound specified for the field.
        /// </summary>
        public double Maximum { get; init; } = double.PositiveInfinity;

        /// <summary>
        /// Whether the minimum is an exclusive minimum.
        /// </summary>
        public bool ExclusiveMinimum { get; init; }
        /// <summary>
        /// Whether the maximum is an exclusive maximum.
        /// </summary>
        public bool ExclusiveMaximum { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldRangeAttribute"/> class.
        /// </summary>
        public FieldRangeAttribute() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldRangeAttribute"/> class with a specified minima/maxima.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        public FieldRangeAttribute(double minimum, double maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            if (schema is not null)
            {
                if (!double.IsNegativeInfinity(Minimum))
                {
                    schema.Minimum = (decimal)Minimum;
                    schema.IsExclusiveMinimum = ExclusiveMinimum;
                }
                if (!double.IsPositiveInfinity(Maximum))
                {
                    schema.Maximum = (decimal)Maximum;
                    schema.IsExclusiveMaximum = ExclusiveMaximum;
                }
            }
            return schema;
        }
    }
}