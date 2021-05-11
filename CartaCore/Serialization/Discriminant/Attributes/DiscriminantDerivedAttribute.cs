using System;

namespace CartaCore.Serialization
{
    /// <summary>
    /// An attribute that can be placed on derived types of an abstract class or interface that specifies a string
    /// value, the discriminant, to differentiate the derived types from each other.
    /// </summary>
    [
        AttributeUsage
        (
            AttributeTargets.Interface |
            AttributeTargets.Class,
            Inherited = true, AllowMultiple = false
        )
    ]
    public class DiscriminantDerivedAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type discriminant.
        /// </summary>
        /// <value>The discriminant which differentiates derived types from each other.</value>
        public string Discriminant { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="DiscriminantDerivedAttribute"/> class with the specified
        /// discriminant.
        /// </summary>
        /// <param name="discriminant">The differentiator between objects with the same base type.</param>
        public DiscriminantDerivedAttribute(string discriminant)
        {
            Discriminant = discriminant;
        }
    }
}