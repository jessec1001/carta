using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CartaCore.Statistics
{
    /// <summary>
    /// Represents a random number generator that can use arbitrary length seeds.
    /// </summary>
    public class CompoundRandom
    {
        private static readonly string Consonants = "bcdfghjklmnpqrstvwxyz";
        private static readonly string Vowels = "aeiou";
        private static readonly double[] LetterFrequency = new double[26]
        {
            0.0780, // Letter a
            0.0200, // Letter b
            0.0400, // Letter c
            0.0380, // Letter d
            0.1100, // Letter e
            0.0140, // Letter f
            0.0300, // Letter g
            0.0230, // Letter h
            0.0860, // Letter i
            0.0021, // Letter j
            0.0097, // Letter k
            0.0530, // Letter l
            0.0270, // Letter m
            0.0720, // Letter n
            0.0610, // Letter o
            0.0280, // Letter p
            0.0019, // Letter q
            0.0730, // Letter r
            0.0870, // Letter s
            0.0670, // Letter t
            0.0330, // Letter u
            0.0100, // Letter v
            0.0091, // Letter w
            0.0027, // Letter x
            0.0160, // Letter y
            0.0044, // Letter z
        };
        private static readonly double ConsonantFrequency = LetterFrequency
            .Where((_, index) => Consonants.Contains((char)(index + 97)))
            .Sum();
        private static readonly double VowelFrequency = LetterFrequency
            .Where((_, index) => Vowels.Contains((char)(index + 97)))
            .Sum();

        private Random[] Randoms { get; set; }

        /// <summary>
        /// Gets the bytes that make up the seed of the random number generator.
        /// </summary>
        /// <value>The bytes of the seed.</value>
        public ReadOnlyCollection<byte> Seed { get; protected set; }
        /// <summary>
        /// Gets the size of the seed in bytes. 
        /// </summary>
        /// <value>The size in bytes of the seed.</value>
        public int SeedSize { get; protected set; }

        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with an array of bytes for a seed.
        /// </summary>
        /// <param name="seed">The seed specified in a byte array.</param>
        public CompoundRandom(byte[] seed)
        {
            Seed = Array.AsReadOnly(seed);
            SeedSize = seed.Length;

            Randoms = new Random[SeedSize / 4];
            byte[] seedParts = new byte[4 * Randoms.Length];
            for (int k = 0; k < seedParts.Length; k++)
            {
                seedParts[k] = k < SeedSize ? Seed[k] : (byte)0;
            }
            for (int k = 0; k < Randoms.Length; k++)
            {
                int individualSeed = BitConverter.ToInt32(seedParts, 4 * k);
                Randoms[k] = new Random(individualSeed);
            }
        }
        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with a specified seed offset by an
        /// identifier.
        /// </summary>
        /// <remarks>
        /// This is used to create deterministic random number generators for unique objects with identities.
        /// </remarks>
        /// <param name="seed">The seed for the random number generator.</param>
        /// <param name="id">The offset identifier that modifies the seed.</param>
        public CompoundRandom(ulong seed, Guid id)
            : this(SeededId(seed, id)) { }
        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with a specified seed.
        /// </summary>
        /// <param name="seed">The seed for the random number generator.</param>
        public CompoundRandom(Guid seed)
            : this(seed.ToByteArray()) { }
        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with a specified seed.
        /// </summary>
        /// <param name="seed">The seed for the random number generator.</param>
        public CompoundRandom(long seed)
            : this((ulong)seed) { }
        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with a specified seed.
        /// </summary>
        /// <param name="seed">The seed for the random number generator.</param>
        public CompoundRandom(ulong seed)
            : this(BitConverter.GetBytes(seed)) { }
        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with a specified seed.
        /// </summary>
        /// <param name="seed">The seed for the random number generator.</param>
        public CompoundRandom(int seed)
            : this((uint)seed) { }
        /// <summary>
        /// Initializes an instance of the <see cref="CompoundRandom"/> class with a specified seed.
        /// </summary>
        /// <param name="seed">The seed for the random number generator.</param>
        public CompoundRandom(uint seed)
            : this(BitConverter.GetBytes(seed)) { }

        private static byte[] SeededId(ulong seed, Guid id)
        {
            byte[] guidBytes = id.ToByteArray();
            ulong a = (BitConverter.ToUInt64(guidBytes, 0) ^ (~seed)) * 0x75c7_55fb_89c3_7073;
            ulong b = (BitConverter.ToUInt64(guidBytes, 8) ^ (seed)) * 0xdf2c_8857_b3e5_3815;
            BitConverter.TryWriteBytes(new Span<byte>(guidBytes, 0, 8), a);
            BitConverter.TryWriteBytes(new Span<byte>(guidBytes, 0, 8), b);
            return guidBytes;
        }

        /// <summary>
        /// Generates a random GUID conforming the UUID v4.
        /// </summary>
        /// <returns>The randomly generated GUID.</returns>
        public Guid NextGuid()
        {
            byte[] guidBytes = new byte[16];
            byte[] randomBytes = new byte[16];

            for (int k = 0; k < Randoms.Length; k++)
            {
                Randoms[k].NextBytes(randomBytes);
                for (int n = 0; n < guidBytes.Length; n++)
                    guidBytes[n] += randomBytes[n];
            }

            // Make sure to specify the version and variant bits.
            // The version is v4. The variant is 1=10_2.
            guidBytes[7] = (byte)(0b01000000 | (0b00001111 & guidBytes[7]));
            guidBytes[9] = (byte)(0b10000000 | (0b00111111 & guidBytes[9]));

            return new Guid(guidBytes);
        }

        /// <summary>
        /// Generates a random integer between 0 (inclusive) and <paramref name="max"/> (exclusive).
        /// </summary>
        /// <param name="max">The maximum (exclusive) value.</param>
        /// <returns>The randomly generated integer.</returns>
        public int NextInt(int max)
        {
            return NextInt(0, max);
        }
        /// <summary>
        /// Generates a random integer between <paramref name="min"/> (inclusive) and <paramref name="max"/>
        /// (exclusive).
        /// </summary>
        /// <param name="min">The minimum (inclusive) value.</param>
        /// <param name="max">The maximum (exclusive) value.</param>
        /// <returns>The randomly generated integer.</returns>
        public int NextInt(int min, int max)
        {
            int diff = max - min;
            if (diff <= 0)
                return 0;

            int result = 0;
            for (int k = 0; k < Randoms.Length; k++)
                result = (result + Randoms[k].Next()) % diff;
            result += min;

            return result;
        }

        /// <summary>
        /// Generates a random floating-point number between <c>0.0</c> (inclusive) and <c>1.0</c> (exclusive).
        /// </summary>
        /// <returns>The randomly generated floating-point number.</returns>
        public double NextDouble()
        {
            double result = 0.0;
            for (int k = 0; k < Randoms.Length; k++)
                result = (result + Randoms[k].NextDouble()) % 1.0;

            return result;
        }

        /// <summary>
        /// Generates a random word made up of randomly generated syllables.
        /// </summary>
        /// <returns>The randomly generated word.</returns>
        public string NextPsuedoword()
        {
            StringBuilder builder = new();

            double syllableProbability = 1.0;
            double syllableDampener = 0.6;

            while (NextDouble() < syllableProbability)
            {
                int syllableType = NextInt(6);
                syllableProbability *= syllableDampener;

                switch (syllableType)
                {
                    case 0:
                        // Closed syllable.
                        builder.Append(NextConsonant());
                        builder.Append(NextVowel());
                        builder.Append(NextConsonant());
                        break;
                    case 1:
                        // Vowel-Consonant-e syllable.
                        builder.Append(NextVowel());
                        builder.Append(NextConsonant());
                        builder.Append('e');
                        break;
                    case 2:
                        // Open syllable.
                        builder.Append(NextConsonant());
                        builder.Append(NextVowel());
                        break;
                    case 3:
                        // Vowel team syllable.
                        builder.Append(NextVowel());
                        builder.Append(NextVowel());
                        break;
                    case 4:
                        // Vower-r syllable.
                        builder.Append(NextVowel());
                        builder.Append('r');
                        break;
                    case 5:
                        // Consonant-le syllable.
                        builder.Append(NextConsonant());
                        builder.Append("le");
                        break;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generates a random vowel based on the distribution of letters in the English corpus.
        /// </summary>
        /// <returns>The randomly generated vowel.</returns>
        public char NextVowel()
        {
            double value = NextDouble() * VowelFrequency;
            double cumulativeFreq = 0;
            for (int k = 0; k < Vowels.Length; k++)
            {
                cumulativeFreq += LetterFrequency[Vowels[k] - 97];
                if (value < cumulativeFreq)
                    return Vowels[k];
            }
            return Vowels.Last();
        }
        /// <summary>
        /// Generates a random consonant based on the distribution of letters in the English corpus.
        /// </summary>
        /// <returns>The randomly generated consonant.</returns>
        public char NextConsonant()
        {
            double value = NextDouble() * ConsonantFrequency;
            double cumulativeFreq = 0;
            for (int k = 0; k < Consonants.Length; k++)
            {
                cumulativeFreq += LetterFrequency[Consonants[k] - 97];
                if (value < cumulativeFreq)
                    return Consonants[k];
            }
            return Consonants.Last();
        }
    }
}