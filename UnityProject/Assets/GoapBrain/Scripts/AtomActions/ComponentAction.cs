using Common;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// A generic action that holds a cached component
    /// </summary>
    public abstract class ComponentAction<T> : GoapAtomAction 
        where T : Component {

        private T cachedComponent;

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

        public override void OnActionOwnerFinished(GoapAgent agent) {
            CacheComponent(agent);
        }

        public override void OnFail(GoapAgent agent) {
            CacheComponent(agent);
        }

        private void CacheComponent(GoapAgent agent) {
            if (this.cachedComponent == null) {
                this.cachedComponent = agent.GetComponent<T>();
                
#if UNITY_EDITOR
                Assertion.NotNull(this.cachedComponent, typeof(T).FullName, agent.gameObject);
#else
                Assertion.NotNull(this.cachedComponent, agent.gameObject);
#endif
            }
        }

    }
}
