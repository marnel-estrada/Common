using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    public class PixelScaler : MonoBehaviour {

        [Tooltip("Pixel width")]
        [SerializeField]
        private int width = 100;

        [Tooltip("Pixel height")]
        [SerializeField]
        private int height = 100;

        [SerializeField]
        private int preferredScreenHeight = 768;

        [SerializeField]
        private float orthographicSize = 1.0f;

        /// <summary>
        /// Applies the pixel scale
        /// May be invoked in editor
        /// </summary>
        public void ApplyPixelScale() {
            float halfScreenHeight = this.preferredScreenHeight * 0.5f;
            float unitsPerPixel = orthographicSize / halfScreenHeight;

            float widthInUnits = this.width * unitsPerPixel;
            float heightInUnits = this.height * unitsPerPixel;

            Vector3 newScale = this.transform.localScale;
            newScale.x = widthInUnits;
            newScale.y = heightInUnits;

            this.transform.localScale = newScale;
        }

    }
}
