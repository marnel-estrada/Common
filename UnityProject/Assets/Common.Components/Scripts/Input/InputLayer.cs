using System.Collections.Generic;

using Common;

using UnityEngine;

namespace Common {
	public class InputLayer : MonoBehaviour {
		[SerializeField]
		private bool modal;

		[SerializeField]
		private bool active;

		[SerializeField]
		private List<InputLayerElement> elements;

		/**
		 * Returns whether or not the input layer is modal or not.
		 */
		public bool IsModal {
			get {
				return this.modal;
			}
		}

		/**
		 * Returns whether or not the input layer is active.
		 */
		public bool IsActive {
			get {
				return this.active;
			}
		}

		/**
		 * Activates the input layer.
		 */
		public void Activate() {
			this.active = true;
		}

		/**
		 * Deactivates the input layer.
		 */
		public void Deactivate() {
			this.active = false;
		}

		/**
		 * Returns whether or not the layer responds to the specified screen touch. Note that the specified Vector3 is treated as screen position.
		 */
		public bool RespondsToTouchPosition(Vector3 touchPos, InputLayer requesterLayer = null) {
			if (this == requesterLayer) {
				// this is already the layer where requester can be found on
				return false;
			}

			for (int i = 0; i < this.elements.Count; ++i) {
				if (this.elements[i].RespondsToTouchPosition(touchPos)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds an element
		/// </summary>
		/// <param name="element"></param>
		public void AddElement(InputLayerElement element) {
			Assertion.IsTrue(!this.elements.Contains(element)); // should not contain the specified element yet
			this.elements.Add(element);
		}

		/// <summary>
		/// Removes an element
		/// </summary>
		/// <param name="element"></param>
		public void RemoveElement(InputLayerElement element) {
			this.elements.Remove(element);
		}
	}
}

