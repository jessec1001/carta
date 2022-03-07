using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Arithmetic
{
    /// <summary>
    /// The input for the <see cref="PrimeFactorOperation" /> operation.
    /// </summary>
    public struct PrimeFactorOperationIn
    {
        /// <summary>
        /// The integer value to compute the prime factorization of. 
        /// </summary>
        [FieldName("Integer")]
        public long Integer { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PrimeFactorOperation" /> operation.
    /// </summary>
    public struct PrimeFactorOperationOut
    {
        /// <summary>
        /// The ordered prime factors of the integer input.
        /// If the input was negative, -1 will be the first factor of the output.
        /// </summary>
        [FieldName("Prime Factors")]
        public IEnumerable<long> Factors { get; set; }
    }

    /// <summary>
    /// Computes the prime factorization of any integer.
    /// The prime factorization of a negative integer starts with a factor of -1.
    /// There is a zero prime factorization of zero.
    /// </summary>
    [OperationName(Display = "Prime Factorization", Type = "primeFactor")]
    [OperationTag(OperationTags.Arithmetic)]
    [OperationTag(OperationTags.NumberTheory)]
    public class PrimeFactorOperation : TypedOperation
    <
        PrimeFactorOperationIn,
        PrimeFactorOperationOut
    >
    {
        private static IEnumerable<long> ComputePrimes(long value)
        {
            // Deal with the case of zero and one.
            if (value == 0)
            {
                yield return 0;
                yield break;
            }
            if (value == 1)
                yield break;

            // Handle negative factorization.
            if (value < 0)
            {
                yield return -1;
                value = -value;
            }

            // Calculate the remaining factors.
            while (value > 1)
            {
                // We only need to check for prime factors up until the square root of the value.
                bool factorFound = false;
                long factorMax = (long)Math.Floor(Math.Sqrt(value));
                for (long k = 2; k <= factorMax; k++)
                {
                    // A factor evenly divides the value.
                    long quotient = Math.DivRem(value, k, out long remainder);
                    if (remainder == 0)
                    {
                        yield return k;
                        factorFound = true;
                        value = quotient;
                        break;
                    }
                }

                // If the value has no factors up to the maximum factor value, then the value is a prime factor itself.
                if (!factorFound)
                {
                    yield return value;
                    value = 1;
                }
            }
        }

        /// <inheritdoc />
        public override Task<PrimeFactorOperationOut> Perform(PrimeFactorOperationIn input)
        {
            // Return the factors.
            return Task.FromResult(new PrimeFactorOperationOut { Factors = ComputePrimes(input.Integer) });
        }
    }
}