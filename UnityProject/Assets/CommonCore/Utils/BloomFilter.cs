using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Common {
    /// <summary>
    ///     Represents a probabilistic data structure that is optimal to
    ///     determine if an object isn't or may be present in a set.
    /// </summary>
    public class BloomFilter {
        private readonly MurMurHash3 hashFunction = new MurMurHash3();

        private readonly int k;
        private readonly int m;

        private BitArray filter;

        /// <summary>
        ///     Initialized a new instance of <see cref="BloomFilter" /> with the specified desired capacity and
        ///     false-positive probability.
        /// </summary>
        /// <param name="n">The approximate amount of objects that the filter will contain.</param>
        /// <param name="p">The false-positive probability.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n" /> or <paramref name="p" /> is out of range.</exception>
        public BloomFilter(int n, double p) {
            if (n < 0) {
                throw new ArgumentOutOfRangeException(nameof(n), n, "n cannot be negative.");
            }

            if (p <= 0 || p >= 1) {
                throw new ArgumentOutOfRangeException(nameof(p), p, "p must be within this range: (0, 1).");
            }

            this.m = EvaluateM(n, p);
            this.k = EvaluateK(this.m, n);

            this.filter = new BitArray(this.m);
        }

        /// <summary>Initialized a new instance of <see cref="BloomFilter" /> with the specified width and depth.</summary>
        /// <param name="m">The length of the array implementing the filter.</param>
        /// <param name="k">The number of hash functions to apply.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="m" /> or <paramref name="k" /> is negative.</exception>
        public BloomFilter(int m, int k) {
            if (m < 0) {
                throw new ArgumentOutOfRangeException(nameof(m), m, "m cannot be negative.");
            }

            if (k < 0) {
                throw new ArgumentOutOfRangeException(nameof(k), k, "k cannot be negative.");
            }

            this.m = m;
            this.k = k;

            this.filter = new BitArray(this.m);
        }

        /// <summary>
        ///     Adds an object to the <see cref="BloomFilter" />.
        ///     <code>Complexity: O(1)</code>
        /// </summary>
        /// <param name="obj">The object to add to the <see cref="BloomFilter" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> is <c>null</c>.</exception>
        public void Add(object obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            int[] hashes = GetHashes(obj, this.m, this.k);

            for (int i = 0; i < this.k; i++) {
                this.filter[hashes[i]] = true;
            }
        }

        /// <summary>
        ///     Determines if the specified object isn't or may be in the <see cref="BloomFilter" />.
        ///     <code>Complexity: O(1)</code>
        /// </summary>
        /// <param name="obj">The object to locate in the <see cref="BloomFilter" />.</param>
        /// <returns>
        ///     <c>false</c> if <paramref name="obj" /> is definitely not in the <see cref="BloomFilter" />, <c>true</c> if it
        ///     may be.
        /// </returns>
        public bool Contains(object obj) {
            int[] hashes = GetHashes(obj, this.m, this.k);

            for (int i = 0; i < this.k; i++) {
                if (!this.filter[hashes[i]]) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Removes all objects from the <see cref="BloomFilter" />.</summary>
        public void Clear() {
            this.filter = new BitArray(this.m);
        }

        #region Helpers

        private static int EvaluateM(double n, double p) {
            return (int) System.Math.Ceiling(-n * System.Math.Log(p) / System.Math.Pow(System.Math.Log(2), 2));
        }

        private static int EvaluateK(double m, double n) {
            return (int) System.Math.Round(m / n * System.Math.Log(2));
        }

        // https://en.wikipedia.org/wiki/Double_hashing
        private int[] GetHashes(object obj, int maxValue, int count) {
            int hash1 = obj.GetHashCode();
            int hash2 = GetHash(obj);

            int[] array = new int[count];

            for (int i = 0; i < count; i++) {
                unchecked {
                    array[i] = System.Math.Abs(hash1 + hash2 * (i + 1)) % maxValue;
                }
            }

            return array;
        }

        private int GetHash(object obj) {
            int output;
            using (MemoryStream stream = new MemoryStream(ObjectToStream(obj))) {
                output = this.hashFunction.Hash(stream);
            }

            return output;
        }

        private static byte[] ObjectToStream(object obj) {
            if (obj == null) {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        #endregion

    }
}