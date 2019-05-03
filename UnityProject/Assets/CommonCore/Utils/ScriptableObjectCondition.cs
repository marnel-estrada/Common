using UnityEngine;

namespace Common {
    /// <summary>
    /// An abstract base class for scriptable objects that implements ICondition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ScriptableObjectCondition : ScriptableObject, ICondition<object> {
        public abstract bool IsMet(object instance);
    }
}