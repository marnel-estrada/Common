using UnityEngine;

namespace Common {
    /// <summary>
    /// Used for objects that are dummy in editor but not needed in runtime
    /// </summary>
    public class Dummy : MonoBehaviour {
        private void Awake() {
            DestroyImmediate(this.gameObject);
        }
    }
}