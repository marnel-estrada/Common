using UnityEngine;

namespace Common {
    /// <summary>
    /// This is useful for web builds so that the game tries to run in the background even while not in focus.
    /// Did it as MonoBehaviour so we don't have to insert this line in some arbitrary existing code.
    /// </summary>
    public class RunInBackground : MonoBehaviour {
        [SerializeField]
        public bool value = true;
        
        private void Awake() {
            Application.runInBackground = value;
        }
    }
}