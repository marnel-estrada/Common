using System;

using Common;

namespace GameEvent {
    // Enum implemented as a struct
    public readonly struct Rarity : IEquatable<Rarity> {
        public static readonly Rarity NULL = new Rarity(0, 0, "Null");
        public static readonly Rarity COMMON = new Rarity(1, 4, "Common");
        public static readonly Rarity UNCOMMON = new Rarity(2, 2, "Uncommon");
        public static readonly Rarity RARE = new Rarity(3, 1, "Rare");

        public static readonly Rarity[] ALL = {
            COMMON, UNCOMMON, RARE
        };

        /// <summary>
        /// Convert from string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Rarity ConvertFromName(string name) {
            for (int i = 0; i < ALL.Length; ++i) {
                if (ALL[i].name.EqualsFast(name)) {
                    return ALL[i];
                }
            }

            Assertion.IsTrue(false, $"Can't convert rarity text: {name}");
            return NULL;
        }

        /// <summary>
        /// Convert from ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Rarity ConvertFromId(int id) {
            for (int i = 0; i < ALL.Length; ++i) {
                if (ALL[i].id == id) {
                    return ALL[i];
                }
            }

            Assertion.IsTrue(false, $"Can't convert rarity id: {id}");
            return NULL;
        }
        
        public readonly int id;
        public readonly int weight;
        public readonly string name;

        private Rarity(int id, int weight, string name) {
            this.id = id;
            this.weight = weight;
            this.name = name;
        }

        public bool Equals(Rarity other) {
            return this.id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is Rarity other && Equals(other);
        }

        public override int GetHashCode() {
            return this.id;
        }

        public static bool operator ==(Rarity left, Rarity right) {
            return left.Equals(right);
        }

        public static bool operator !=(Rarity left, Rarity right) {
            return !left.Equals(right);
        }
    }
}