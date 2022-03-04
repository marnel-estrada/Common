using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common {
    [RequireComponent(typeof(Button))]
    public class ButtonRepeat : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        [SerializeField]
        private float repeatDelay = 1f;

        [SerializeField]
        private float repeatClickInterval = 0.1f;

        private Button? button;
        private PointerEventData? pointerData;
        private float lastTrigger;

        private void Awake() {
            this.button = this.GetRequiredComponent<Button>();
        }

        private void Update() {
            if (this.pointerData != null && this.button != null) {
                if (Time.realtimeSinceStartup - this.lastTrigger >= this.repeatDelay) {
                    this.lastTrigger = Time.realtimeSinceStartup - (this.repeatDelay - this.repeatClickInterval);
                    this.button.OnSubmit(this.pointerData);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            this.lastTrigger = Time.realtimeSinceStartup;
            this.pointerData = eventData;
        }

        public void OnPointerUp(PointerEventData eventData) {
            this.pointerData = null;
            this.lastTrigger = 0f;
        }
    }
}