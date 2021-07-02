namespace Common {
    /// <summary>
    /// A utility class that caches an int game variable
    /// </summary>
    public class IntGameVariable : GameVariable<int> {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public IntGameVariable(string key) : base(key) {
        }

        protected override int ResolveValue(string key) {
            return GameVariablesQuery.GetInt(key);
        }
    }
}
