namespace Common {
    /// <summary>
    /// A utility class that caches a string game variable
    /// </summary>
    public class StringGameVariable : GameVariable<string> {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public StringGameVariable(string key) : base(key) {
        }

        protected override string ResolveValue(string key) {
            return GameVariablesQuery.Get(key);
        }

    }
}
