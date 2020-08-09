using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /**
     * An element used by Input Layer to identify whether it responds to a certain touch position or not. It uses
     * colliders for now. We can refactor it to become an interface if needed later on.
     */
    public class InputLayerElement : MonoBehaviour {

        [SerializeField] // for debugging purposes
        private Camera referenceCamera;

        [SerializeField]
        private string referenceCameraName;

        [SerializeField]
        private Collider[] touchColliders;

        [SerializeField]
        private Collider2D[] touchColliders2d;

        private Camera ReferenceCamera {
            get {
                // we lazy initialize so that we don't have a problem when to get this instance
                if (this.referenceCamera == null) {
                    this.referenceCamera = UnityUtils.GetRequiredComponent<Camera>(this.referenceCameraName);
                }

                return this.referenceCamera;
            }
        }

        /**
         * Returns whether or not the element responds to the specified touch position.
         */
        public bool RespondsToTouchPosition(Vector3 touchPos) {
            if (this.ReferenceCamera == null) {
                // may not be resolved like it is deactivated
                return false;
            }

            Ray touchRay = this.ReferenceCamera.ScreenPointToRay(touchPos);
            foreach (Collider collider in this.touchColliders) {
                if (collider.Raycast(touchRay, out RaycastHit hit, 1000)) {
                    return true;
                }
            }

            if (this.touchColliders2d != null && this.touchColliders2d.Length > 0) {
                Vector3 worldTouch = this.ReferenceCamera.ScreenToWorldPoint(touchPos);
                Vector2 worldTouch2d = new Vector2(worldTouch.x, worldTouch.y);
                for (int i = 0; i < this.touchColliders2d.Length; ++i) {
                    if (this.touchColliders2d[i].OverlapPoint(worldTouch2d)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Sets the colliders
         * This was made so that editor code could set this data
         */
        public void SetColliders(List<Collider> colliderList) {
            this.touchColliders = new Collider[colliderList.Count];

            for (int i = 0; i < colliderList.Count; ++i) {
                this.touchColliders[i] = colliderList[i];
            }
        }
    }
}