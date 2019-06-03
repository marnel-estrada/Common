using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Refactored common way of transforming screen input to world position
    /// </summary>
    public class CameraPointerInput : MonoBehaviour {

        [SerializeField]
        private string referenceCameraName = "Gui1Camera";

        private Camera referenceCamera;

        private void Awake() {
            Assertion.AssertNotEmpty(this.referenceCameraName);
            this.referenceCamera = UnityUtils.GetRequiredComponent<Camera>(this.referenceCameraName);
            this.referenceCameraName = null; // Free memory
        }

        public Vector3 PointerWorldPosition {
            get {
                Vector3 position = this.referenceCamera.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;
                return position;
            }
        }

    }
}
