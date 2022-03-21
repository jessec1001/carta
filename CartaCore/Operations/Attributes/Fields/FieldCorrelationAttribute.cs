using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that specifies that a field is correlated with another field.
    /// </summary>
    public class FieldCorrelationAttribute : Attribute
    {
        /// <summary>
        /// The field in the opposite field side that this field is correlated with.
        /// </summary>
        public string Correlated { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCorrelationAttribute"/> class.
        /// </summary>
        /// <param name="correlated">The field in the opposite field side that this field correlates to.</param>
        public FieldCorrelationAttribute(string correlated) => Correlated = correlated;
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCorrelationAttribute"/> class.
        /// </summary>
        public FieldCorrelationAttribute() : this(null) { }
    }
}