using System;
using System.Diagnostics.CodeAnalysis;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents an identity for a freeform object.
    /// </summary>
    public abstract class FreeformIdentity : IEquatable<FreeformIdentity>, IComparable<FreeformIdentity>
    {
        /// <inheritdoc />
        public abstract bool Equals([AllowNull] FreeformIdentity other);
        /// <inheritdoc />
        public abstract int CompareTo([AllowNull] FreeformIdentity other);

        /// <summary>
        /// Determines whether the identity can be converted to the specified type.
        /// </summary>
        /// <param name="id">The identity.</param>
        /// <param name="result">The resulting object of the specified type.</param>
        /// <typeparam name="T">The type to convert the identity to.</typeparam>
        /// <returns><c>true</c> if the identity is of the specified type; otherwise, <c>false</c>.</returns>
        public static bool IsType<T>(FreeformIdentity id, out T result) where T : IEquatable<T>, IComparable<T>
        {
            if (id is FreeformConstantIdentity<T> typedId)
            {
                result = typedId.Identity;
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }
        /// <summary>
        /// Creates a constant identity from the specified object.
        /// </summary>
        /// <param name="obj">The object to convert into an identity.</param>
        /// <typeparam name="T">The type to convert.</typeparam>
        /// <returns>The created constant identity.</returns>
        public static FreeformIdentity Create<T>(T obj) where T : IEquatable<T>, IComparable<T>
        {
            return new FreeformConstantIdentity<T>(obj);
        }
    }
}