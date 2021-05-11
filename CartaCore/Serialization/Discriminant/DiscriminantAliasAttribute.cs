using System;

namespace CartaCore.Serialization
{
    /// <summary>
    /// An attribute that can be placed on parameterless, static methods that construct a discriminant object with type
    /// equal to the enclosing type. The alias name is used in the same way that discriminant names are used to
    /// differentiate types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DiscriminantAliasAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type alias.
        /// </summary>
        /// <value>The alias which specifies a specific parameterization of a discriminant type.</value>
        public string AliasName { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="DiscriminantAliasAttribute"/> class with the specified alias.
        /// </summary>
        /// <param name="aliasName">The alias of the discriminant parameterization.</param>
        public DiscriminantAliasAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }
}