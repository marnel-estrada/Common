namespace GoapBrain {
    /// <summary>
    /// An abstract base class for classes that resolves a condition value
    /// </summary>
    public abstract class ConditionResolver {
        private bool resolved;
        private bool conditionMet;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ConditionResolver() {
            Reset();
        }

        /// <summary>
        /// Resets the resolver
        /// May be needed during planning
        /// </summary>
        public void Reset() {
            this.resolved = false;
            this.conditionMet = false;
        }

        /// <summary>
        /// Returns whether or not the condition is met
        /// </summary>
        /// <returns></returns>
        public bool IsMet(GoapAgent agent) {
            if (!this.resolved) {
                // Not yet resolved
                this.conditionMet = Resolve(agent);
                this.resolved = true;
            }

            return this.conditionMet;
        }

        public bool Resolved {
            get {
                return this.resolved;
            }
        }

        /// <summary>
        /// Resolves the value of the condition
        /// </summary>
        /// <returns></returns>
        protected abstract bool Resolve(GoapAgent agent);
    }
}