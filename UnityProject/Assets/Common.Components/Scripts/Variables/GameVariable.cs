namespace Common {
    /// <summary>
    /// Abstract base class for objects that can query a game variable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GameVariable<T> {
        private readonly string key;
        private bool valueResolved;
        private T value;

        public GameVariable(string key) {
            this.key = key;
            this.valueResolved = false;
        }

        public T Value {
            get {
                if (!this.valueResolved) {
                    // value is not yet resolved
                    this.value = ResolveValue(this.key);
                    this.valueResolved = true;
                }

                return this.value;
            }
        }

        // Resolves the value of the specified key
        protected abstract T ResolveValue(string key);

        /// <summary>
        /// This is called to disable domain reload
        /// </summary>
        public void Reset() {
            this.valueResolved = false;
        }
    }
}
