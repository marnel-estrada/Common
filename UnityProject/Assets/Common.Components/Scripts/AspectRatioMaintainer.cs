using UnityEngine;

namespace Common {
    /// <summary>
    /// Maintains aspect ratio in different resolutions.
    /// </summary>
    public class AspectRatioMaintainer : MonoBehaviour {
        [SerializeField]
        private int targetResolutionWidth = 9;
        
        [SerializeField]
        private int targetResolutionHeight = 16;

        [SerializeField]
        private new Camera camera; // This is the camera to alter

        private void Start() {
            Assertion.NotNull(this.camera);
            
            float targetAspect = this.targetResolutionWidth / (float)this.targetResolutionHeight;
            float currentAspect = Screen.width / (float)Screen.height;
            float rectHeight = currentAspect / targetAspect;

            Rect cameraRect = this.camera.rect;
            cameraRect.height = rectHeight;
            cameraRect.y = (1.0f - rectHeight) / 2.0f;
            this.camera.rect = cameraRect;
        }

        private void OnPreRender() {
            GL.Clear(true, true, ColorUtils.BLACK); 
        }
    }
}