using System;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a unique identity for an object.
    /// </summary>
    public abstract class Identity : IEquatable<Identity>, IComparable<Identity>
    {
        /// <inheritdoc />
        public abstract int CompareTo(Identity other);

        /// <inheritdoc />
        public abstract bool Equals(Identity other);

        /// <summary>
        /// Determines whether the identity can be converted to the specified type.
        /// </summary>
        /// <param name="result">The resulting object of the specified type.</param>
        /// <typeparam name="T">The type to convert the identity to.</typeparam>
        /// <returns><c>true</c> if the identity is of the specified type; otherwise, <c>false</c>.</returns>
        public abstract bool IsType<T>(out T result) where T : IEquatable<T>, IComparable<T>;
        /// <summary>
        /// Converts the identity to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the identity to.</typeparam>
        /// <returns>The converted type if possible to convert; otherwise, <c>null</c>.</returns>
        public virtual T AsType<T>() where T : IEquatable<T>, IComparable<T>
        {
            IsType(out T result);
            return result;
        }

        /// <summary>
        /// Creates a constant identity from the specified object.
        /// </summary>
        /// <param name="obj">The object to convert into an identity.</param>
        /// <typeparam name="T">The type to convert.</typeparam>
        /// <returns>The created constant identity.</returns>
        public static Identity Create<T>(T obj) where T : IEquatable<T>, IComparable<T>
        {
            return new ConstantIdentity<T>(obj);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Identity);
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}