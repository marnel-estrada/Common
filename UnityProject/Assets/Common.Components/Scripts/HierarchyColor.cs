using UnityEngine;

namespace Common {
	/// <summary>
	/// This component is used in conjunction with HeirarchyColorCode
	/// It only stores the color of the heirarchy background to render with
	/// </summary>
	public class HierarchyColor : MonoBehaviour {

		[SerializeField]
		private Color color = Color.white;

		void Awake() {
#if !UNITY_EDITOR
			// we don't really need this component at runtime, so we destroy it
			Destroy(this);
#endif
		}

		public Color Color {
			get {
				return this.color;
			}
		}

	}
}
