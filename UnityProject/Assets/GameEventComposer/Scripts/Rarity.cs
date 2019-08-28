using Common;

namespace GameEvent {
    // Enum implemented as a struct
    public readonly struct Rarity {
        public static readonly Rarity NULL = new Rarity(0, "Null");
        public static readonly Rarity COMMON = new Rarity(1, "Common");
        public static readonly Rarity UNCOMMON = new Rarity(2, "Uncommon");
        public static readonly Rarity RARE = new Rarity(3, "Rare");

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
                if (ALL[i].name.Equals(name)) {
                    return ALL[i];
                }
            }

            Assertion.Assert(false, $"Can't convert rarity text: {name}");
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

            Assertion.Assert(false, $"Can't convert rarity id: {id}");
            return NULL;
        }
        
        public readonly int id;
        public readonly string name;

        private Rarity(int id, string name) {
            this.id = id;
            this.name = name;
        }
    }
}