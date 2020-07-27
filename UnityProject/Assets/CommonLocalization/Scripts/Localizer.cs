using UnityEngine;

namespace Common {
    /// <summary>
    /// Abstract base class for all localizer classes
    /// </summary>
    public abstract class Localizer : MonoBehaviour {
        [SerializeField]
        private string termId;

        private bool addedToManager = false;

        // We keep an instance of this so we don't invoke LocalizerManager.Instance in OnDestroy().
        // This is bad because LocalizerManager.Instance creates a GameObject. Will cause problems if
        // application is being closed
        private LocalizerManager manager;

        protected virtual void Awake() {
            AddToManager();
        }

        protected virtual void OnEnable() {
            AddToManager();
        }

        private void AddToManager() {
            if (this.addedToManager) {
                // Already added
                return;
            }

            if (this.manager == null) {
                this.manager = LocalizerManager.Instance;
            }

            this.manager.Add(this);
            this.addedToManager = true;
        }

        protected virtual void OnDestroy() {
            this.manager.Remove(this);
            this.addedToManager = false;
        }

        public string TermId {
            get {
                return this.termId;
            }

            set {
                this.termId = value;
            }
        }

        /// <summary>
        /// Updates the text
        /// </summary>
        public abstract void UpdateText(Option<string> newText);
    }
}