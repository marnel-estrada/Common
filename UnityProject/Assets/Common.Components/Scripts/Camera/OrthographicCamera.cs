using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthographicCamera : MonoBehaviour {
	
	private Camera selfCamera;
	private IList<OrthographicCameraObserver> observerList;
	
	void Awake() {
		this.observerList = new List<OrthographicCameraObserver>();
	}
	
	void Start() {
		// cache
		this.selfCamera = this.GetComponent<Camera>();
	}
	
	/**
	 * Adds an observer.
	 */
	public void AddObserver(OrthographicCameraObserver observer) {
		this.observerList.Add(observer);
	}
	
	/**
	 * Sets a new orthosize.
	 */
	public void SetOrthoSize(float size) {
		this.selfCamera.orthographicSize = size;
		
		// invoke observers
		foreach(OrthographicCameraObserver observer in observerList) {
			observer.OnChangeOrthoSize(size);
		}
	}
	
}
