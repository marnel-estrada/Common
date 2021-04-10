using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// A generic action that holds a cached component
    /// </summary>
    public abstract class OptionalComponentAction<T> : GoapAtomAction
        where T : Component {

        private T cachedComponent;
        private bool resolved;

        /// <summary>
        /// Returns the cached component so it can be used by a deriving class
        /// </summary>
        protected T CachedComponent {
            get {
                return this.cachedComponent;
            }
        }

        public override GoapResult Start(GoapAgent agent) {
            CacheComponent(agent);
            return GoapResult.SUCCESS; // Deriving class should decide whether or not it will return success or running
        }

        public override void OnFail(GoapAgent agent) {
            CacheComponent(agent);
        }

        private void CacheComponent(GoapAgent agent) {
            // Resolve only once
            if (this.cachedComponent == null && !this.resolved) {
                this.cachedComponent = agent.GetComponent<T>();
                this.resolved = true;
            }
        }

    }
}
