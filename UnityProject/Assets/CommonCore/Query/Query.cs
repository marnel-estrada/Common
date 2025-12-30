namespace Common {
    using System;

    /// <summary>
    /// A generic query such that boxing/unboxing is avoided
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Query<TRequest, TResult> {
        private Func<TRequest, TResult>? provider;

        /// <summary>
        /// Registers a provider
        /// </summary>
        /// <param name="provider"></param>
        public void RegisterProvider(Func<TRequest, TResult> provider) {
            Assertion.IsTrue(this.provider == null); // Avoid more than one provider
            this.provider = provider;
        }

        public bool HasProvider => this.provider != null;

        /// <summary>
        /// Executes the query
        /// Returns the expected result
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TResult Execute(TRequest request) {
            if (this.provider == null) {
                throw new Exception("Provider was not specified yet");
            }
            
            return this.provider(request);
        }

        /// <summary>
        /// Executes the query without the request
        /// There may be queries that needs no parameters
        /// </summary>
        /// <returns></returns>
        public TResult Execute() {
            if (this.provider == null) {
                throw new Exception("Provider was not specified yet");
            }
            
            return this.provider(default!);
        }

        // Do not remove this even if not called. This is invoked by reflection such as StaticFieldsInvoker.
        public void ClearProvider() {
            this.provider = null;
        }
    }
}
