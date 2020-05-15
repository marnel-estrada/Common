using UnityEngine;
using UnityEngine.UI;

namespace Common {
	/**
	 * Displays a frame rate using a UI Text.
	 */
	public class FrameRateView : MonoBehaviour {

	    [SerializeField]
		private Text text;

		private FrameRate frameRate;

		private void Awake() {
	        Assertion.AssertNotNull(this.text);
			this.frameRate = new FrameRate();
		}

		private void Update() {
			this.frameRate.Update(UnityEngine.Time.deltaTime);
			this.text.text = this.frameRate.GetFrameRate().ToString();
		}
	}
}
