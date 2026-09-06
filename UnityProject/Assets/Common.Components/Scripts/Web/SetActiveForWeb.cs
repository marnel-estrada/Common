using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System;
#endif

namespace Common.Web {
    public class SetActiveForWeb : MonoBehaviour {
        [SerializeField]
        private GameObject[] activateTargets;
        
        [SerializeField]
        private GameObject[] deactivateTargets;

        private void Awake() {
#if UNITY_WEBGL && !UNITY_EDITOR
            foreach (GameObject target in this.activateTargets) {
                target.SetActive(true);
            }

            foreach (GameObject target in this.deactivateTargets) {
                target.SetActive(false);
            }
#endif
        }
    }
}