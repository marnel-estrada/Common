using Common;

namespace GoapBrain {
    /// <summary>
    /// The database for all condition names
    /// This is to reduce usage of condition as string which bloats memory
    /// Implemented as a singleton
    /// </summary>
    class ConditionNamesDatabase {

        private StringDatabase stringDb = new StringDatabase(1000);

        /// <summary>
        /// Used private constructor because this is a singleton
        /// </summary>
        private ConditionNamesDatabase() {
        }

        /// <summary>
        /// Adds a condition name and returns its ID
        /// </summary>
        /// <param name="conditionName"></param>
        /// <returns></returns>
        public ConditionId GetOrAdd(string conditionName) {
            return new ConditionId(this.stringDb.Add(conditionName));
        }

        /// <summary>
        /// Returns the string associated with the specified condition id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetName(ConditionId id) {
            return this.stringDb.GetString(id.Value);
        }

        private static ConditionNamesDatabase INSTANCE = null;

        public static ConditionNamesDatabase Instance {
            get {
                if(INSTANCE == null) {
                    INSTANCE = new ConditionNamesDatabase();
                }

                return INSTANCE;
            }
        }

    }
}
