using UnityEngine;
using System.Collections.Generic;

namespace Common {
	public class InputLayerStack : MonoBehaviour {
		// the layer at zero is the top of the stack
		[SerializeField]
		private InputLayer[] initialLayers;

		// QQQ serialize for debugging only
		[SerializeField]
		private InputLayer topLayer;

		// implemented as a list as we will be traversing the list whenever we push or pop
		// QQQ serialize for debugging only
		[SerializeField]
		private List<InputLayer> layerStack;

		void Awake() {
			// populate with initial layers
			this.layerStack = new List<InputLayer>();
			ResetStack();
		}

		/**
     		 * Resets the input layer stack.
     		 * Note that we don't use the name Reset() since it is invoked by Unity editor
     		 */
		public void ResetStack() {
			Clear();

			for (int i = this.initialLayers.Length - 1; i >= 0; --i) {
				Assertion.NotNull(this.initialLayers[i]);
				Push(this.initialLayers[i]);
			}
		}

		/**
     		 * Pushes an input layer.
     		 */
		public void Push(InputLayer layer) {
			if (!IsEmpty() && layer == Top()) {
				// the specified layer is already at the top
				return;
			}

			this.layerStack.Add(layer);
			EnsureModality();

			// QQQ for debugging only
			this.topLayer = Top();
		}

		/**
     		 * Pops the top layer.
     		 */
		public void Pop() {
			if (IsEmpty()) {
				// there are no layers in the stack
				return;
			}

			InputLayer layerToBeRemoved = Top();
			layerToBeRemoved.Deactivate();

			this.layerStack.RemoveAt(this.layerStack.Count - 1);
			EnsureModality();

			if (!IsEmpty()) {
				this.topLayer = Top();
			}
		}

		/// <summary>
		/// Ensures the modality of the layers based on the top most layer
		/// </summary>
		public void EnsureModality() {
			bool activateLayer = true;
			for (int i = this.layerStack.Count - 1; i >= 0; --i) {
				// the topmost layer is always activated since we set activateLayer as true
				InputLayer layer = this.layerStack[i];
				if (activateLayer) {
					layer.Activate();
				} else {
					layer.Deactivate();
				}

				// if the current layer is modal, then the succeeding layers are deactivated
				if (activateLayer && layer.IsModal) {
					activateLayer = false;
				}
			}
		}

		/**
     		 * Returns the InputLayer instance that is at the top.
     		 */
		public InputLayer Top() {
			Assertion.IsTrue(!IsEmpty(), "Can't get top if stack is empty.");

			return this.layerStack[this.layerStack.Count - 1];
		}

		/**
     		 * Returns whether or not the stack is empty.
     		 */
		public bool IsEmpty() {
			return this.layerStack.Count == 0;
		}

		/**
     		 * Clears the stack of layers.
     		 */
		public void Clear() {
			while (!IsEmpty()) {
				Pop();
			}
		}

		/**
     		 * Returns whether or not there is an active layer from the top that could respond to the specified screen touch position.
     		 * This is used to block input if a layer does indeed responds to the specified touch position.
     		 */
		public bool RespondsToTouchPosition(Vector3 touchPos, InputLayer requesterLayer = null) {
			for (int i = this.layerStack.Count - 1; i >= 0; --i) {
				InputLayer layer = this.layerStack[i];

				if (layer == requesterLayer) {
					// The layer of the requester has already been reached and it was not blocked by layers above it
					return false;
				}

				if (layer.IsActive && layer.RespondsToTouchPosition(touchPos)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the specified layer
		/// </summary>
		/// <param name="layer"></param>
		public void Remove(InputLayer layer) {
			if (this.layerStack.Contains(layer)) {
				// just following Pop() functionality
				layer.Deactivate();
			}

			this.layerStack.Remove(layer);
			EnsureModality();
		}

		/// <summary>
		/// Removes the layer from the top of stack while also checking for modal layers along the way
		/// We allow removal of non modal layers on top of a modal one
		/// </summary>
		/// <param name="layer"></param>
		public void RemoveFromTop(InputLayer layer) {
			if (IsEmpty()) {
				// Nothing to remove
				return;
			}

			for (int i = this.layerStack.Count - 1; i >= 0; --i) {
				if (this.layerStack[i] == layer) {
					// We found the layer to remove
					this.layerStack.RemoveAt(i);
					layer.Deactivate();

					EnsureModality();

					if (!IsEmpty()) {
						this.topLayer = Top();
					}

					return;
				}

				// We no longer check if a layer would be removed that is past a model layer
				// We just let be removed
			}
		}
	}
}

