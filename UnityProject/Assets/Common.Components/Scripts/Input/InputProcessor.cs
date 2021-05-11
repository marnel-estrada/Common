using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
	public class InputProcessor : MonoBehaviour {
		[SerializeField]
		private List<string>? inputLayerNameList;
		
		private readonly SimpleList<InputLayer> inputLayerList = new SimpleList<InputLayer>();
		
		[SerializeField]
		private string? referenceCameraName; // note that we are using a string because the actual camera object may be found in another scene

		private Camera? referenceCamera;
		
		public delegate void InputProcess();
		
		private InputProcess? inputProcess;

        // This is usually used to cancel existing mouse related states when input is no longer valid
        // Like for example, when mouse moves to an area that covers this InputProcessor
        private InputProcess? inputRemovedProcess; // may be null

		// We keep an instance of this so we don't invoke InputProcessorManager.Instance in OnDestroy().
		// This is bad because InputProcessorManager.Instance creates a GameObject. Will cause problems if
		// application is being closed
		private InputProcessorManager? manager;

        private void Awake() {
	        Assertion.NotEmpty(this.referenceCameraName);
	        
	        this.manager = InputProcessorManager.Instance;
            this.manager.Add(this);
        }

        private void OnDestroy() {
	        if (this.manager != null) {
		        this.manager.Remove(this);
	        }
        }

        private void Start() {
			// populate input layer list
			if(this.inputLayerNameList != null && this.inputLayerNameList.Count > 0) {
				foreach(string layerName in this.inputLayerNameList) {
					InputLayer layer = UnityUtils.GetRequiredComponent<InputLayer>(layerName);
					this.inputLayerList.Add(layer);
				}
			}
			
			// resolve reference camera
	        this.referenceCamera = UnityUtils.GetRequiredComponent<Camera>(this.referenceCameraName ?? throw new InvalidOperationException());
		}
		
        /// <summary>
        /// Update routine
        /// </summary>
		public void ExecuteUpdate() {
			if(!CanProcessInput()) {
				this.inputRemovedProcess?.Invoke(); // invoke
                return;
			}
            
			if(this.inputProcess != null) {
                // Everything is ok, let it process input
                // We catch exceptions here so that input processing will not stop if the process fails.
                try {
                    this.inputProcess();
                } catch(Exception e) {
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
			}
		}
	
		/**
		 * Returns whether or not it can process input
		 */
		public bool CanProcessInput() {
			// Can process input if there's an active layer
			// Input elements are not processed for now
			return GetActiveLayer().IsSome;
		}
		
		private Option<InputLayer> GetActiveLayer() {
			int count = this.inputLayerList.Count;
            for (int i = 0; i < count; ++i) {
                if(this.inputLayerList[i].IsActive) {
                    return Option<InputLayer>.Some(this.inputLayerList[i]);
                }
            }
			
			return Option<InputLayer>.NONE;
		}
		
		/**
		 * Sets the input processor.
		 */
		public void SetInputProcess(InputProcess newInputProcess) {
			this.inputProcess = newInputProcess;
		}

        /// <summary>
        /// Sets the routine when input is removed 
        /// Like for instance, when mouse moves to an area that covers this InputProcessor
        /// </summary>
        /// <param name="process"></param>
        public void SetInputRemovedProcess(InputProcess process) {
            this.inputRemovedProcess = process;
        }
		
		public Camera ReferenceCamera {
			get {
				if (this.referenceCamera != null) {
					return this.referenceCamera;
				}

				throw new Exception("ReferenceCamera can't be null");
			}
		}
	}
}
