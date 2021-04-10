using System.Collections.Generic;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Handles a list of shiny objects
    /// </summary>
    class ShinyObjectPool : MonoBehaviour {

        [SerializeField]
        private List<ShinyObject> objectList = new List<ShinyObject>();

        /// <summary>
        /// Adds a shiny object
        /// </summary>
        /// <param name="shinyObject"></param>
        public void Add(ShinyObject shinyObject) {
            this.objectList.Add(shinyObject);
        }

        /// <summary>
        /// Gets an unclaimed shiny object
        /// </summary>
        /// <returns></returns>
        public ShinyObject GetUnclaimed() {
            for (int i = 0; i < this.objectList.Count; ++i) {
                if (!this.objectList[i].Claimed) {
                    return this.objectList[i];
                }
            }

            // Client code should check for this
            return null;
        }

        /// <summary>
        /// Removes a shiny object
        /// </summary>
        /// <param name="shinyObject"></param>
        public void Remove(ShinyObject shinyObject) {
            this.objectList.Remove(shinyObject);
        }

    }
}
