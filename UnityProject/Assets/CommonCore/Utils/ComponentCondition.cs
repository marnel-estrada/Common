using UnityEngine;

namespace Common {
    /// <summary>
    /// An abstract base class for components that implements ICondition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ComponentCondition<T> : MonoBehaviour, ICondition<T> {
        public abstract bool IsMet(T instance);
    }
}