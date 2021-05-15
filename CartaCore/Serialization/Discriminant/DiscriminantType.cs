using System;

namespace CartaCore.Serialization
{
    /// <summary>
    /// Represents a discriminant type and its properties derived from attributes applied to the type.
    /// </summary>
    public class DiscriminantType
    {
        /// <summary>
        /// Gets or sets the discriminant type.
        /// </summary>
        /// <value>The derived type underlying the discriminant.</value>
        public Type Type { get; init; }

        /// <summary>
        /// Gets or sets the discriminant value.
        /// </summary>
        /// <value>The discriminating value.</value>
        public string Discriminant { get; init; }

        /// <summary>
        /// Gets or sets whether the discriminant is hiddden.
        /// </summary>
        /// <value><c>true</c> if the discriminant should be enumerated over; otherwise, <c>false</c>.</value>
        public bool Hidden { get; init; }
        /// <summary>
        /// Gets or sets the discriminant display name.
        /// </summary>
        /// <value>The human-readable display name of the discriminant.</value>
        public string Name { get; init; }
        /// <summary>
        /// Gets or sets the discriminant group name.
        /// </summary>
        /// <value>The human-readable display name of the group of the discriminant.</value>
        public string Group { get; init; }
    }
}