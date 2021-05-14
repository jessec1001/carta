using System;

namespace CartaCore.Serialization
{
    /// <summary>
    /// Represents a discriminant alias, its properties, and its generator derived from attributes applied to the type.
    /// </summary>
    public class DiscriminantAlias : DiscriminantType
    {
        /// <summary>
        /// Gets or sets the alias generator.
        /// </summary>
        /// <value>The function that generates an alias value.</value>
        public Func<object> Generator { get; init; }
    }
}