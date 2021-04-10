using UnityEngine;

namespace GoapBrain {
    class ShinyObject : MonoBehaviour {

        private bool claimed = false;

        public bool Claimed {
            get {
                return claimed;
            }

            set {
                this.claimed = value;
            }
        }

    }
}
