namespace Common {
    /// <summary>
    /// A utility class that caches a bool game variable
    /// </summary>
    public class BoolGameVariable : GameVariable<bool> {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public BoolGameVariable(string key) : base(key) {
        }

        protected override bool ResolveValue(string key) {
            return GameVariablesQuery.GetBool(key);
        }

    }
}
