using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the performance of the <see cref="PrimeFactorOperation" /> operation.
    /// </summary>
    public class TestPrimeFactorOperation
    {
        /// <summary>
        /// Tests that the prime factor operation factors integers correctly.
        /// </summary>
        /// <param name="expected">The expected prime factorization.</param>
        /// <param name="integer">The integer to factor.</param>
        [TestCase(new long[] { }, 0)]
        [TestCase(new long[] { 1 }, 1)]
        [TestCase(new long[] { 2 }, 2)]
        [TestCase(new long[] { 3 }, 3)]
        [TestCase(new long[] { 2, 2 }, 4)]
        [TestCase(new long[] { 5 }, 5)]
        [TestCase(new long[] { 2, 2, 2, 3, 5 }, 120)]
        [TestCase(new long[] { 43, 47 }, 2021)]
        [TestCase(new long[] { -1, 17, 31, 997 }, -1 * 31 * 17 * 997)]
        public async Task TestFactorization(long[] expected, long integer)
        {
            PrimeFactorOperation operation = new();
            PrimeFactorOperationIn input = new() { Integer = integer };
            PrimeFactorOperationOut output = await operation.Perform(input);

            Assert.AreEqual(expected, output.Factors);
        }
    }
}