using UnityEngine;

namespace Common.Signal {
    [RequireComponent(typeof(SignalIntegrityHandler))]
    public abstract class SignalHandlerComponent : MonoBehaviour {
        private SignalIntegrityHandler signalIntegrity;

        protected virtual void Awake() {
            this.signalIntegrity = this.GetRequiredComponent<SignalIntegrityHandler>();
        }

        /// <summary>
        /// Adds a signal listener
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="listener"></param>
        protected void AddSignalListener(Signal signal, Signal.SignalListener listener) {
            Assertion.NotNull(this.signalIntegrity, "You might have forgotten to call base.Awake().");
            this.signalIntegrity.Add(signal, listener);
        }
    }
}