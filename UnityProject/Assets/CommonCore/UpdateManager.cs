using System;

using UnityEngine;

namespace Common {
    /// <summary>
    /// A common manager that 
    /// We need this as mentioned here https://blogs.unity3d.com/2015/12/23/1k-update-calls/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpdateManager<T> : MonoBehaviour where T : Component {
        private Action<T> updateAction;

        private readonly SimpleList<T> entries = new SimpleList<T>(100);

        /// <summary>
        /// Initializer
        /// Must be invoked before Update()
        /// </summary>
        /// <param name="updateAction"></param>
        protected virtual void Init(Action<T> updateAction) {
            this.updateAction = updateAction;
            Assertion.NotNull(this.updateAction);
        }

        /// <summary>
        /// Adds an update entry
        /// </summary>
        /// <param name="entry"></param>
        public void Add(T entry) {
            this.entries.Add(entry);
        }

        /// <summary>
        /// Removes an update entry
        /// </summary>
        /// <param name="entry"></param>
        public void Remove(T entry) {
            this.entries.Remove(entry);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update() {
            Assertion.NotNull(this.updateAction);

            int count = this.entries.Count;
            for (int i = 0; i < count; ++i) {
                T entry = this.entries[i];
                if(!entry.gameObject.activeInHierarchy) {
                    // Not active
                    continue;
                }

                // Invoke the update action
                this.updateAction(entry);
            }
        }

    }
}
