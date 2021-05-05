namespace Common.Signal {
    /// <summary>
    /// Manages a instances of signal-listener pair which handles remove later on
    /// This was refactored from SignalIntegrityHandler so it could be used in non MonoBehaviour environment like editor
    /// </summary>
    public class SignalIntegrity {

        private readonly SimpleList<SignalListenerPair> pairList;

        /// <summary>
        /// Constructor
        /// </summary>
        public SignalIntegrity() {
            this.pairList = new SimpleList<SignalListenerPair>();
        }

        /**
		 * Adds a signal and listener pair to be maintained
		 */
        public void Add(Signal signal, Signal.SignalListener listener) {
            SignalListenerPair pair = new SignalListenerPair(signal, listener);
            pair.AddListener(); // automatically add the listener to the signal
            this.pairList.Add(pair);
        }

        /**
         * Detaches the listeners to their signal and removes all signal-listener pair
         */
        public void Clear() {
            // remove all listeners from their signals
            for (int i = 0; i < this.pairList.Count; ++i) {
                this.pairList[i].RemoveListener();
            }

            this.pairList.Clear();
        }

    }
}
