using System;
using System.Collections;

namespace Common {
    /// <summary>
    ///     Represents a probabilistic data structure that is optimal to
    ///     determine if an object isn't or may be present in a set.
    /// </summary>
    public class BloomFilter<T> where T : IEquatable<T> {
        private readonly int k;
        private readonly int m;

        private BitArray filter;

        private readonly int[] hashes;

        // The byte array representation of the BloomFilter
        // This is useful for serialization
        private readonly byte[] byteArray;

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
            this.hashes = new int[this.k];
            
            this.byteArray = new byte[(this.filter.Length - 1) / 8 + 1];
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
        /// <param name="item">The object to add to the <see cref="BloomFilter" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is <c>null</c>.</exception>
        public void Add(T item) {
            if (item == null) {
                // Do not add null items
                return;
            }

            ComputeHashes(item, this.m);

            for (int i = 0; i < this.k; i++) {
                this.filter[this.hashes[i]] = true;
            }
        }

        /// <summary>
        ///     Determines if the specified object isn't or may be in the <see cref="BloomFilter" />.
        ///     <code>Complexity: O(1)</code>
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="BloomFilter" />.</param>
        /// <returns>
        ///     <c>false</c> if <paramref name="item" /> is definitely not in the <see cref="BloomFilter" />, <c>true</c> if it
        ///     may be.
        /// </returns>
        public bool Contains(T item) {
            ComputeHashes(item, this.m);

            for (int i = 0; i < this.hashes.Length; i++) {
                if (!this.filter[this.hashes[i]]) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Removes all objects from the <see cref="BloomFilter" />.</summary>
        public void Clear() {
            this.filter.SetAll(false);
        }
        
        public byte[] ByteArray {
            get {
                this.filter.CopyTo(this.byteArray, 0);
                return this.byteArray;
            }
        }

        public void Set(byte[] byteArray) {
            this.filter = new BitArray(byteArray);
        }

        private static int EvaluateM(double n, double p) {
            return (int) Math.Ceiling(-n * Math.Log(p) / Math.Pow(Math.Log(2), 2));
        }

        private static int EvaluateK(double m, double n) {
            return (int) Math.Round(m / n * Math.Log(2));
        }

        private void ComputeHashes(T item, int maxValue) {
            // Zero out values first
            for (int i = 0; i < this.hashes.Length; ++i) {
                this.hashes[i] = 0;
            }

            int doubledHash = item.GetHashCode() << 1;

            for (int i = 0; i < this.hashes.Length; i++) {
                unchecked {
                    this.hashes[i] = Math.Abs(doubledHash * (i + 1)) % maxValue;
                }
            }
        }
    }
}