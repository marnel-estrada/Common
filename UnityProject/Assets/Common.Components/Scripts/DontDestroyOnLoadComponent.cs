using UnityEngine;

namespace Common {
	/**
	 * A general component that may be added to objects to mark them as DontDestroyOnLoad.
	 */
	public class DontDestroyOnLoadComponent : MonoBehaviour {
		private void Awake() {
			DontDestroyOnLoad(this.gameObject);
		}
	}
}
