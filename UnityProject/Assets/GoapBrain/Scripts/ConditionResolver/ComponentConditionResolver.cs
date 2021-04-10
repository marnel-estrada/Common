using Common;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// A generic condition resolver that caches a component
    /// </summary>
    public abstract class ComponentConditionResolver<T> : ConditionResolver where T : Component {
        private T cachedComponent;

        protected T CachedComponent {
            get {
                return this.cachedComponent;
            }
        }

        protected override bool Resolve(GoapAgent agent) {
            CacheComponent(agent);
            return false;
        }

        protected void CacheComponent(GoapAgent agent) {
            if(this.cachedComponent == null) {
                this.cachedComponent = agent.GetComponent<T>();
                Assertion.NotNull(this.cachedComponent, agent.gameObject);
            }
        }
    }
}
