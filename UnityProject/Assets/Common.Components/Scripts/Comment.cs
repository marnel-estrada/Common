using UnityEngine;

namespace Common {
	/**
	 * A component that holds a comment so we could see it in the inspector
	 */
	public class Comment : MonoBehaviour {
		[SerializeField]
		private string text;

		private void Awake() {
			DestroyImmediate(this); // auto destroy to save memory
		}

		public string Text {
			get => this.text;

			set => this.text = value;
		}
	}
}
