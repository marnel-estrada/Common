namespace Common.Signal {
    /// <summary>
    /// Another signal implementation that uses typed parameters
    /// This is to avoid boxing
    ///
    /// We only limit parameters to struct so that users of this wouldn't pass references or
    /// create small objects from classes which is garbage.
    /// </summary>
    public class TypedSignal<T> where T : struct {
        public delegate void SignalListener(T parameter);
        
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
        public void Dispatch(T parameter) {
            int listenersCount = this.listeners.Count;
            for (int i = 0; i < listenersCount; ++i) {
                this.listeners[i](parameter); // Invoke the delegate
            }
        }
    }
}