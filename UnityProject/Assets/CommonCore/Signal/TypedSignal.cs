namespace Common.Signal {
    /// <summary>
    /// Another signal implementation that uses typed parameters
    /// This is to avoid boxing
    /// </summary>
    public class TypedSignal<TParameter> where TParameter : struct {

        public delegate void SignalListener(TParameter parameter);
        
        private readonly SimpleList<SignalListener> listeners = new SimpleList<SignalListener>(1);

        public void AddListener(SignalListener listener) {
            Assertion.Assert(!this.listeners.Contains(listener)); // Prevent duplicate listeners
            this.listeners.Add(listener);
        }

        public void RemoveListener(SignalListener listener) {
            this.listeners.Remove(listener);
        }

        /// <summary>
        /// Invokes all listeners to the signal
        /// </summary>
        public void Dispatch(TParameter parameter) {
            int listenersCount = this.listeners.Count;
            for (int i = 0; i < listenersCount; ++i) {
                this.listeners[i](parameter); // Invoke the delegate
            }
        }

    }
}