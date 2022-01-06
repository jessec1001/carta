using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="PrimeFactorOperation" /> operation.
    /// </summary>
    public struct PrimeFactorOperationIn
    {
        /// <summary>
        /// The integer value to compute the prime factorization of. 
        /// </summary>
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
        public long[] Factors { get; set; }
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
        /// <inheritdoc />
        public override Task<PrimeFactorOperationOut> Perform(PrimeFactorOperationIn input)
        {
            // Deal with the case of zero and one.
            long value = input.Integer;
            if (value == 0)
                return Task.FromResult(new PrimeFactorOperationOut { Factors = new long[] { 0 } });
            if (value == 1)
                return Task.FromResult(new PrimeFactorOperationOut { Factors = Array.Empty<long>() });

            // Store the factors in a list and convert later.
            List<long> factors = new();

            // Handle negative factorization.
            if (value < 0)
            {
                factors.Add(-1);
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
                        factors.Add(k);
                        factorFound = true;
                        value = quotient;
                        break;
                    }
                }

                // If the value has no factors up to the maximum factor value, then the value is a prime factor itself.
                if (!factorFound)
                {
                    factors.Add(value);
                    value = 1;
                }
            }

            // Return the factors.
            return Task.FromResult(new PrimeFactorOperationOut { Factors = factors.ToArray() });
        }
    }
}