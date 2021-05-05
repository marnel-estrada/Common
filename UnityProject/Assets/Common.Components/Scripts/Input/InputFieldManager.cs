using System;

using UnityEngine;
using UnityEngine.UI;

using Common.Signal;

namespace Common {
    /// <summary>
    /// Handles input fields such that InputLayerManager can check if there's a focused input layer and should not process input layers.
    /// </summary>
    public class InputFieldManager : MonoBehaviour {
        // Serialized for debugging only
        [SerializeField]
        private bool hasFocus;

        private readonly SimpleList<InputField> inputFields = new SimpleList<InputField>(5);
        
        private static readonly Signal.Signal REGISTER_INPUT_FIELD = new Signal.Signal("RegisterInputField");
        
        private static readonly StaticFieldsInvoker CLEAR_LISTENERS = new StaticFieldsInvoker(typeof(InputFieldManager), "ClearListeners");
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDomainReload() {
            CLEAR_LISTENERS.Execute();
        }
        
        private const string INPUT_FIELD_PARAM = "InputField";

        private Action<InputField> addMatcher;

        /// <summary>
        /// Registers an input field
        /// </summary>
        /// <param name="inputField"></param>
        public static void RegisterInputField(InputField inputField) {
            REGISTER_INPUT_FIELD.ClearParameters();
            REGISTER_INPUT_FIELD.AddParameter(INPUT_FIELD_PARAM, inputField);
            REGISTER_INPUT_FIELD.Dispatch();
        }

        private void Awake() {
            this.addMatcher = delegate(InputField inputField) {
                this.inputFields.Add(inputField);
            };
            
            REGISTER_INPUT_FIELD.AddListener(Register);
        }

        private void Register(ISignalParameters parameters) {
            Option<InputField> inputField = parameters.GetParameter<InputField>(INPUT_FIELD_PARAM);
            inputField.Match(this.addMatcher);
        }

        private void Update() {
            // Check if any one of the input fields has focus
            int count = this.inputFields.Count;
            for (int i = 0; i < count; ++i) {
                if(this.inputFields[i].isFocused) {
                    this.hasFocus = true;
                    return;
                }
            }

            this.hasFocus = false;
        }

        public bool HasFocus {
            get {
                return this.hasFocus;
            }
        }
    }
}