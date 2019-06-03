using System;

using UnityEngine;

namespace Common {
    public sealed class UrlOpener : MonoBehaviour {

        [SerializeField]
        private string url; // May or may not be specified

        /// <summary>
        /// Opens the specified URL
        /// This can also be used as a button handler
        /// </summary>
        public void Open() {
            if (!string.IsNullOrEmpty(this.url)) {
                Application.OpenURL(this.url);
            }
        }

    }
}
