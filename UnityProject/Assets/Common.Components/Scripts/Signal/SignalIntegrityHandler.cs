using Common.Signal;

using UnityEngine;

namespace Common.Signal {
    /**
     * Handles the integrity of signals during the lifetime of a GameObject.
     */
    public class SignalIntegrityHandler : MonoBehaviour {
        private readonly SimpleList<SignalListenerPair> pairList = new SimpleList<SignalListenerPair>(1);

        /**
         * Adds a signal and listener pair to be maintained
         */
        public void Add(Signal signal, Signal.SignalListener listener) {
            SignalListenerPair pair = new SignalListenerPair(signal, listener);
            pair.AddListener(); // automatically add the listener to the signal
            this.pairList.Add(pair);
        }

        void OnDestroy() {
            // remove all listeners from their signals
            for(int i = 0; i < this.pairList.Count; ++i) {
                this.pairList[i].RemoveListener();
            }

            this.pairList.Clear();
        }
    }
}
