//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

using UnityEngine;

namespace Common {
	/**
	 * Contains color related utility methods.
	 */
	public static class ColorUtils {
		// we provide these static colors because color variables from Color are instantiated instead of static
		public static readonly Color BLACK = new Color(0, 0, 0, 1);
		public static readonly Color WHITE = new Color(1, 1, 1, 1);
		public static readonly Color RED = new Color(1, 0, 0, 1);
		public static readonly Color GREEN = new Color(0, 1, 0, 1);
        public static readonly Color BLUE = new Color(0, 0, 1, 1);
        public static readonly Color YELLOW = new Color(1, 1, 0, 1);

		public static readonly Color BLACK_ZERO_ALPHA = new Color(0, 0, 0, 0);

        /**
		 * Contructs a new color from RGBA components
		 */
        public static Color NewColor(int r, int g, int b, int a) {
			return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}

        /// <summary>
        /// Parses a color from a hex text
        /// </summary>
        /// <param name="hexColorText"></param>
        /// <returns></returns>
        public static Color Parse(string hexColorText) {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(hexColorText, out color);

            return color;
        }

        public static Color WithAlpha(this Color self, float alpha) {
	        return new Color(self.r, self.g, self.b, alpha);
        }

        public static bool TolerantEquals(this Color self, Color other) {
	        return self.r.TolerantEquals(other.r) &&
	               self.g.TolerantEquals(other.g) &&
	               self.b.TolerantEquals(other.b) &&
	               self.a.TolerantEquals(other.a);
        }
	}
}
