using UnityEngine;
using System.Collections;

/**
 * A general component that may be added to objects to mark them as DontDestroyOnLoad.
 */
public class DontDestroyOnLoadComponent : MonoBehaviour {

	void Awake() {
		DontDestroyOnLoad(this.gameObject);
	}
	
}

