using System;

namespace CartaCore.Serialization
{
    /// <summary>
    /// An attribute that can be placed on discriminant types and aliases that specifies human-readable values for a
    /// name and a group. This is purely used for debugging and display purposes.
    /// </summary>
    [
        AttributeUsage
        (
            AttributeTargets.Interface |
            AttributeTargets.Class |
            AttributeTargets.Method,
            Inherited = false, AllowMultiple = false
        )
    ]
    public class DiscriminantSemanticsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether the discriminant is hidden.
        /// </summary>
        /// <value><c>true</c> if the discriminant should be hidden from querying; otherwise <c>false</c>.</value>
        public bool Hidden { get; init; }

        /// <summary>
        /// Gets or sets the discriminant name.
        /// </summary>
        /// <value>A human-readable display name for the discriminant.</value>
        public string Name { get; init; }
        /// <summary>
        /// Gets or sets the discriminant name.
        /// </summary>
        /// <value>A human-readable group name for the discriminant.</value>
        public string Group { get; init; }
    }
}