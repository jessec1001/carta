using System;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents an object that contains an identity that can be used to order and assert this object in relation to
    /// other objects of its kind.
    /// </summary>
    /// <typeparam name="T">The type of object that requires an identity.</typeparam>
    public interface IIdentifiable<T> : IEquatable<T>, IComparable<T> where T : IIdentifiable<T>
    {
        /// <summary>
        /// Gets or sets the identifier that uniquely represents this object. 
        /// </summary>
        /// <value>The identifier of this object. This value cannot be changed after initialization.</value>
        Identity Identifier { get; }
    }
}