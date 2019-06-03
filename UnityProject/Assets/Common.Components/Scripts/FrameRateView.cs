using Common;

using UnityEngine;
using UnityEngine.UI;

/**
 * Displays a frame rate using a UI Text.
 */
public class FrameRateView : MonoBehaviour {

    [SerializeField]
	private Text text;

	private FrameRate frameRate;
	
	void Awake() {
        Assertion.AssertNotNull(this.text);
		this.frameRate = new FrameRate();
	}
	
	void Update() {
		frameRate.Update(Time.deltaTime);
		text.text = this.frameRate.GetFrameRate().ToString();
	}

}
