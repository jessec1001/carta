using System;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// An attribute that allows specifying that a parameter to an API endpoint is actually parsed and should have a
    /// differently specified underlying type.
    /// </summary>
    [
        AttributeUsage
        (
            AttributeTargets.Class |
            AttributeTargets.Enum |
            AttributeTargets.Struct |
            AttributeTargets.Interface,
            Inherited = false, AllowMultiple = true
        )
    ]
    sealed class ApiTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the underlying type for the parameter.
        /// </summary>
        /// <value>The type that will be expected from requests before model binding.</value>
        public Type UnderlyingType { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="ApiTypeAttribute"/> class with the specified underlying
        /// type.
        /// </summary>
        /// <param name="underlyingType">The type that will be expected from requests before model binding.</param>
        public ApiTypeAttribute(Type underlyingType = null)
        {
            UnderlyingType = underlyingType;
        }
    }
}