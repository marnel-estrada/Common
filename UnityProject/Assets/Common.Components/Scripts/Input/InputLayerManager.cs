using System;

using UnityEngine;
using System.Collections.Generic;

using Common.Signal;

namespace Common {
    public class InputLayerManager : MonoBehaviour {
        [SerializeField]
        private InputLayerStack layerStack;

        private Dictionary<string, InputLayer> foundLayers;

        // Signals
        public static readonly Signal.Signal PUSH_INPUT_LAYER = new Signal.Signal("PushInputLayer");
        public static readonly Signal.Signal POP_INPUT_LAYER = new Signal.Signal("PopInputLayer");
        public static readonly Signal.Signal ACTIVATE_INPUT_LAYER = new Signal.Signal("ActivateInputLayer");
        public static readonly Signal.Signal DEACTIVATE_INPUT_LAYER = new Signal.Signal("DeactivateInputLayer");
        
        private static readonly StaticFieldsInvoker CLEAR_LISTENERS = new StaticFieldsInvoker(typeof(InputLayerManager), "ClearListeners");
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDomainReload() {
            CLEAR_LISTENERS.Execute();
        }

        // Parameters
        public const string INPUT_LAYER_NAME = "InputLayerName";

        private Action<string> pushLayerMatcher;
        private Action<string> popLayerMatcher;
        private Action<string> activateMatcher;
        private Action<string> deactivateMatcher;

        private void Awake() {
            Assertion.NotNull(this.layerStack);

            this.foundLayers = new Dictionary<string, InputLayer>();
            
            this.pushLayerMatcher = delegate(string inputLayerName) {
                this.layerStack.Push(ResolveInputLayer(inputLayerName));
            };
            
            this.popLayerMatcher = delegate(string inputLayerName) {
                // we ensure that client code knows which input layer to pop
                // that's why we require the input layer name when popping
                this.layerStack.RemoveFromTop(ResolveInputLayer(inputLayerName));
            };
            
            this.activateMatcher = delegate(string inputLayerName) {
                ResolveInputLayer(inputLayerName).Activate();
            };
            
            this.deactivateMatcher = delegate(string inputLayerName) {
                ResolveInputLayer(inputLayerName).Deactivate();
            };

            PUSH_INPUT_LAYER.AddListener(PushInputLayer);
            POP_INPUT_LAYER.AddListener(PopInputLayer);
            ACTIVATE_INPUT_LAYER.AddListener(ActivateInputLayer);
            DEACTIVATE_INPUT_LAYER.AddListener(DeactivateInputLayer);
        }

        private void OnDestroy() {
            PUSH_INPUT_LAYER.RemoveListener(PushInputLayer);
            POP_INPUT_LAYER.RemoveListener(PopInputLayer);
            ACTIVATE_INPUT_LAYER.RemoveListener(ActivateInputLayer);
            DEACTIVATE_INPUT_LAYER.RemoveListener(DeactivateInputLayer);
        }

        private void PushInputLayer(ISignalParameters parameters) {
            Option<string> inputLayerName = parameters.GetParameter<string>(INPUT_LAYER_NAME);
            inputLayerName.Match(this.pushLayerMatcher);
        }

        private void PopInputLayer(ISignalParameters parameters) {
            Assertion.IsTrue(!this.layerStack.IsEmpty()); // should not be empty when popping

            Option<string> inputLayerName = parameters.GetParameter<string>(INPUT_LAYER_NAME);
            inputLayerName.Match(this.popLayerMatcher);
        }

        private void ActivateInputLayer(ISignalParameters parameters) {
            Option<string> inputLayerName = parameters.GetParameter<string>(INPUT_LAYER_NAME);
            inputLayerName.Match(this.activateMatcher);
        }

        private void DeactivateInputLayer(ISignalParameters parameters) {
            Option<string> inputLayerName = parameters.GetParameter<string>(INPUT_LAYER_NAME);
            inputLayerName.Match(this.deactivateMatcher);
        }

        // resolves the input layer with the specified name
        private InputLayer ResolveInputLayer(string name) {
            if (this.foundLayers.ContainsKey(name)) {
                return this.foundLayers[name];
            }

            // not cached yet, so we look for it
            InputLayer found = UnityUtils.GetRequiredComponent<InputLayer>(name);
            this.foundLayers[name] = found;

            return found;
        }

        /**
		 * Returns whether or not there is an active layer from the top that could respond to the specified screen touch position.
		 * This is used to block input if a layer does indeed responds to the specified touch position.
		 */
        public bool RespondsToTouchPosition(Vector3 touchPos, InputLayer requesterLayer = null) {
            return this.layerStack.RespondsToTouchPosition(touchPos, requesterLayer);
        }

        public static void PushInputLayer(string inputLayerName) {
            Signal.Signal signal = PUSH_INPUT_LAYER;
            signal.ClearParameters();
            signal.AddParameter(INPUT_LAYER_NAME, inputLayerName);
            signal.Dispatch();
        }
        
        public static void PopInputLayer(string inputLayerName) {
            Signal.Signal signal = POP_INPUT_LAYER;
            signal.ClearParameters();
            signal.AddParameter(INPUT_LAYER_NAME, inputLayerName);
            signal.Dispatch();
        }
    }
}