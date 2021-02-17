using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents an identity that is compared on the basis of a constant value.
    /// </summary>
    /// <typeparam name="T">The type of equatable and comparable constant value.</typeparam>
    public class FreeformConstantIdentity<T> : FreeformIdentity, IEquatable<T>, IComparable<T>
        where T : IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Gets or sets the identity value.
        /// </summary>
        /// <value>The identitiy.</value>
        public T Identity { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformConstantIdentity{T}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier object.</param>
        public FreeformConstantIdentity(T id)
        {
            Identity = id;
        }

        /// <summary>
        /// Tries to convert a source string into a typed result using reflection to find a <c>TryParse</c> on the
        /// specified type's class.
        /// </summary>
        /// <param name="source">The source string to convert.</param>
        /// <param name="result">
        /// The converted result if successful. If not successful, set to <c>default(U)</c>.
        /// </param>
        /// <typeparam name="U">The type to convert the string to.</typeparam>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        private bool TryParse<U>(string source, out U result)
        {
            // Set default result in case we cannot convert.
            result = default(U);

            // If source is null, simply return false.
            if (source is null) return false;

            // Try to get a 'TryParse' method that we can use to produce the conversion.
            MethodInfo tryparse = typeof(U).GetMethod
            (
                nameof(TryParse),
                new[] { typeof(string), typeof(U).MakeByRefType() }
            );
            if (!(tryparse is null) && tryparse.ReturnType == typeof(bool))
            {
                object[] args = new object[] { source, null };
                bool success = (bool)tryparse.Invoke(null, args);
                result = (U)args.Last();

                return success;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] T other)
        {
            return Identity.Equals(other);
        }
        /// <inheritdoc />
        public override bool Equals([AllowNull] FreeformIdentity other)
        {
            if (other is FreeformConstantIdentity<T> constantOther) return Identity.Equals(constantOther.Identity);
            if (other is FreeformConstantIdentity<string> stringOther &&
                TryParse(stringOther.Identity, out T parsedOther)) return Identity.Equals(parsedOther);
            else return other.Equals(this);
        }
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is FreeformIdentity other) return Equals(other);
            if (obj is T otherIdentity) return Equals(otherIdentity);
            return false;
        }

        /// <inheritdoc />
        public int CompareTo([AllowNull] T other)
        {
            return Identity.CompareTo(other);
        }
        /// <inheritdoc />
        public override int CompareTo([AllowNull] FreeformIdentity other)
        {
            if (other is FreeformConstantIdentity<T> constantOther) return Identity.CompareTo(constantOther.Identity);
            if (other is FreeformConstantIdentity<string> stringOther &&
                TryParse(stringOther.Identity, out T parsedOther)) return Identity.CompareTo(parsedOther);
            return other.CompareTo(this);
        }

        /// <inheritdoc />
        public override bool IsType<U>(out U result)
        {
            // First, try to directly convert underlying types.
            // Second, try to parse a string into the type.
            // Otherwise, identity cannot be converted to specified type.
            if (Identity is U typed)
            {
                result = typed;
                return true;
            }
            else if (Identity is string str && TryParse(str, out U parsed))
            {
                result = parsed;
                return true;
            }
            else
            {
                result = default(U);
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Identity.ToString();
        }
    }
}