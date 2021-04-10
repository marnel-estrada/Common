using Common;

using UnityEngine;

namespace GoapBrain {
    public abstract class OptionalComponentConditionResolver<T> : ConditionResolver where T : Component {
        private Option<T> cachedComponent;
        private bool resolved;
        
        /// <summary>
        /// Returns the cached component so it can be used by a deriving class
        /// </summary>
        protected Option<T> CachedComponent {
            get {
                return this.cachedComponent;
            }
        }

        protected override bool Resolve(GoapAgent agent) {
            CacheComponent(agent);
            return false;
        }
        
        private void CacheComponent(GoapAgent agent) {
            // Resolve only once
            if (this.resolved) {
                return;
            }

            T component = agent.GetComponent<T>();
            this.cachedComponent = component == null ? Option<T>.NONE : Option<T>.Some(component);
            this.resolved = true;
        }
    }
}