using UnityEngine;

namespace Common {
    public class CanvasCameraResolver : MonoBehaviour {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private string cameraName;

        private void Awake() {
            Assertion.NotNull(this.canvas);
            Assertion.NotEmpty(this.cameraName);
            
            // set the new camera as specified in cameraName
            Camera resolvedCamera = UnityUtils.GetRequiredComponent<Camera>(this.cameraName);
            if (resolvedCamera != null) {
                // deactivate the current camera so it won't render
                if(this.canvas.worldCamera != null) {
                    this.canvas.worldCamera.gameObject.SetActive(false);
                }

                // note here that we set camera only when it is found
                // this is so we could still play the scene by itself without relying with camera from other scenes
                this.canvas.worldCamera = resolvedCamera;
            }
        }
    }
}