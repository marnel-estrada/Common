using Common;

using UnityEngine;

public class HudOrthoAutoScale : MonoBehaviour, OrthographicCameraObserver {
	
	[SerializeField]
	private Camera selfCamera;
	private OrthographicCamera orthoCamera;
	
	private Transform selfTransform;
	private float originalOrthoSize;
	private float prevOrthoSize;
	
	private Vector3 originalPosition;
	private Vector3 originalScale;
	
	void Start() {
		// assign the main camera if camera was not assigned
		if(selfCamera == null) {
			this.selfCamera = Camera.main;
		}
		
		Assertion.Assert(this.selfCamera.orthographic, "The assigned camera must be orthographic.");
		
		this.orthoCamera = this.selfCamera.GetComponent<OrthographicCamera>();
		
		// register self as observer if existing
		if(orthoCamera != null) {
			this.orthoCamera.AddObserver(this);
		}
		
		this.selfTransform = this.transform;
		this.originalOrthoSize = this.selfCamera.orthographicSize;
		this.prevOrthoSize = this.selfCamera.orthographicSize;
		
		this.originalPosition = this.selfTransform.position;
		this.originalScale = this.selfTransform.localScale;
	}
	
	void Update() {
		/*if(Comparison.TolerantEquals(camera.orthographicSize, prevOrthoSize)) {
			return;
		}
		
		// ortho size changed, we update transform
		this.prevOrthoSize = camera.orthographicSize;
		float scale = this.prevOrthoSize / this.originalOrthoSize;
		
		Vector3 newScale = this.originalScale * scale;
		this.selfTransform.localScale = newScale;
		
		Vector3 newPosition = this.originalPosition * scale;
		newPosition.z = originalPosition.z; // we don't update Z as to remain consistent to the whole scene
		this.selfTransform.position = newPosition;*/
	}
	
	//#region OrthographicCameraObserver implementation
	public void OnChangeOrthoSize (float newSize) {
		// ortho size changed, we update transform
		this.prevOrthoSize = newSize;
		float scale = this.prevOrthoSize / this.originalOrthoSize;
		
		Vector3 newScale = this.originalScale * scale;
		this.selfTransform.localScale = newScale;
		
		Vector3 newPosition = this.originalPosition * scale;
		newPosition.z = originalPosition.z; // we don't update Z as to remain consistent to the whole scene
		this.selfTransform.position = newPosition;
	}
	//#endregion
}

