using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;

namespace Common.Signal {
    /**
	 * This is an immutable data class
	 */
    public class SignalListenerPair {
        private readonly Signal signal;
        private readonly Signal.SignalListener listener;

        /**
         * Constructor
         */
        public SignalListenerPair(Signal signal, Signal.SignalListener listener) {
            this.signal = signal;
            this.listener = listener;
        }

        /**
         * Adds the listener to the signal
         */
        public void AddListener() {
            this.signal.AddListener(this.listener);
        }

        /**
         * Removes the listener from the signal
         */
        public void RemoveListener() {
            this.signal.RemoveListener(this.listener);
        }
    }
}
