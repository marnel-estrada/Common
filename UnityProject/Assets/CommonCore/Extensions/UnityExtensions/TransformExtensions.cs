//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

using UnityEngine;

namespace Common {
	/**
	 * Contains extension functions to Transform
	 */
	public static class TransformExtensions {

		/// <summary>
        /// Sets the X scale
        /// </summary>
        /// <param name="self"></param>
        /// <param name="xScale"></param>
		public static void SetXScale(this Transform self, float xScale) {
			Vector3 newScale = self.localScale;
			newScale.x = xScale;
			self.localScale = newScale;
		}

        /// <summary>
        /// Sets the Y scale
        /// </summary>
        /// <param name="self"></param>
        /// <param name="yScale"></param>
        public static void SetYScale(this Transform self, float yScale) {
            Vector3 newScale = self.localScale;
            newScale.y = yScale;
            self.localScale = newScale;
        }

        /// <summary>
        /// Sets the Z local position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z"></param>
        public static void SetLocalZPosition(this Transform self, float z) {
            self.localPosition = new Vector3(self.localPosition.x, self.localPosition.y, z);
        }

        /// <summary>
        /// Sets the X position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z"></param>
        public static void SetXPosition(this Transform self, float x) {
            self.position = new Vector3(x, self.position.y, self.position.z);
        }

        /// <summary>
        /// Sets the Y position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z"></param>
        public static void SetYPosition(this Transform self, float y) {
            self.position = new Vector3(self.position.x, y, self.position.z);
        }

        /// <summary>
        /// Sets the Z position
        /// </summary>
        /// <param name="self"></param>
        /// <param name="z"></param>
        public static void SetZPosition(this Transform self, float z) {
            self.position = new Vector3(self.position.x, self.position.y, z);
        }

		/// <summary>
		/// Sets the Y local position
		/// </summary>
		/// <param name="self"></param>
		/// <param name="y"></param>
		public static void SetLocalYPosition(this Transform self, float y) {
			self.localPosition = new Vector3(self.localPosition.x, y, self.localPosition.z);
		}

        /// <summary>
        /// Sets the local Y euler angle
        /// </summary>
        /// <param name="self"></param>
        /// <param name="y"></param>
        public static void SetLocalYEuler(this Transform self, float y) {
	        Vector3 eulerAngles = self.localEulerAngles;
	        eulerAngles.y = y;
	        self.eulerAngles = eulerAngles;
        }

        /// <summary>
        /// Sets the local z euler rotation
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rotation"></param>
        public static void SetLocalZEulerAngle(this Transform self, float rotation) {
            Vector3 euler = self.localEulerAngles;
            euler.z = rotation;
            self.localEulerAngles = euler;
        }

        /// <summary>
        /// Traverses the tranform tree using the specified visitor
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="visitor"></param>
        /// <typeparam name="T"></typeparam>
        public static void Traverse<T>(this Transform transform, T visitor) where T : struct, IVisitor<Transform> {
	        visitor.Visit(transform);

	        for (int i = 0; i < transform.childCount; ++i) {
		        Transform child = transform.GetChild(i);
		        Traverse<T>(child, visitor);
	        }
        }
	}
}
