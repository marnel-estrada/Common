using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    [ExecuteInEditMode]
    public class PixelTransform : MonoBehaviour {

        [SerializeField]
        private int x = 0;

        [SerializeField]
        private int y = 0;

        [SerializeField]
        private float orthographicSize = 1.0f;

        [SerializeField]
        private int targetResolutionHeight = 768;

        private Transform cachedTransform;

        void Reset() {
            // set pixel positions to that of current transform
            Vector3 worldPosition = this.transform.position;
            this.x = Mathf.FloorToInt(worldPosition.x / this.UnitsPerPixel);
            this.y = Mathf.FloorToInt(worldPosition.y / this.UnitsPerPixel);

            SnapToPixelPosition();
        }

        void Awake() {
            this.cachedTransform = this.transform;
        }
        
        void Update() {
            SnapToPixelPosition();
        }

        private float UnitsPerPixel {
            get {
                return this.orthographicSize / (this.targetResolutionHeight >> 1); // divide 2
            }
        }

        private void SnapToPixelPosition() {
            Vector3 newPosition = VectorUtils.ZERO;

            Transform localTransform = this.cachedTransform;
            if(localTransform == null) {
                // this may be the case if it was running in editor
                // note that the component has ExecuteInEditMode attribute
                localTransform = this.transform;
            }

            float unitsPerPixel = this.UnitsPerPixel;
            newPosition.x = this.x * unitsPerPixel;
            newPosition.y = this.y * unitsPerPixel;
            newPosition.z = localTransform.position.z; // just copy

            localTransform.position = newPosition;
        }

        /// <summary>
        /// Sets the pixel position using world position
        /// </summary>
        /// <param name="world"></param>
        public void SetFromWorldPosition(Vector3 world) {
            this.x = Mathf.FloorToInt(world.x / this.UnitsPerPixel);
            this.y = Mathf.FloorToInt(world.y / this.UnitsPerPixel);

            SnapToPixelPosition();
        }

    }
}
