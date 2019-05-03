using System;
using System.Collections.Generic;

namespace Common {
    public class QueryManagerImplementation {
        
        private delegate object QueryProvider(QueryRequest request); // The internal delegate that we manage

        private readonly Dictionary<Type, QueryProvider> providerMap = new Dictionary<Type, QueryProvider>();

        /// <summary>
        /// Registers a provider
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="provider"></param>
        public void RegisterProvider<R, V>(QueryManager.QueryProvider<R, V> provider) where R : QueryRequest {
            Type type = typeof(R);
            Assertion.Assert(!this.providerMap.ContainsKey(type)); // Should not contain the provider for a certain request yet

            // Make the internal delegate which invokes the generic delegate
            object InternalProvider(QueryRequest request) {
                return provider((R) request);
            }

            this.providerMap[type] = InternalProvider;
        }

        /// <summary>
        /// Removes the provider of the specified request type
        /// </summary>
        /// <typeparam name="R"></typeparam>
        public void RemoveProvider<R>() {
            this.providerMap.Remove(typeof(R));
        }

        /// <summary>
        /// Returns wheter or not there's a registered provider for the specified request
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        public bool HasProvider<R>() where R : QueryRequest {
            return this.providerMap.ContainsKey(typeof(R));
        }

        /// <summary>
        /// Queries for a value
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public V Query<R, V>(R request) where R : QueryRequest {
            Type type = typeof(R);

            // Invoke the provider
            // This will throw an error if a provider does not exist
            return (V)this.providerMap[type](request);
        }

    }
}
