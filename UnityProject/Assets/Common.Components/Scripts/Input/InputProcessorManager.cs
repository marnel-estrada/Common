using UnityEngine;

namespace Common {
    [RequireComponent(typeof(InputFieldManager))]
    public class InputProcessorManager : UpdateManager<InputProcessor> {
        private InputFieldManager? inputFieldManager;

        private void Awake() {
            this.inputFieldManager = this.GetRequiredComponent<InputFieldManager>();
            
            // Set the update action
            Init(delegate (InputProcessor processor) {
                if(this.inputFieldManager.HasFocus) {
                    // An input field currently has focus. Do not proceed to processing.
                    return;
                }

                processor.ExecuteUpdate();
            });
        }

        private static InputProcessorManager? INSTANCE;

        public static InputProcessorManager Instance {
            get {
                if(INSTANCE == null) {
                    GameObject go = new GameObject("InputProcessorManager");
                    go.AddComponent<DontDestroyOnLoadComponent>();
                    INSTANCE = go.AddComponent<InputProcessorManager>();
                }

                return INSTANCE;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDomainReload() {
            INSTANCE = null;
        }
    }
}
