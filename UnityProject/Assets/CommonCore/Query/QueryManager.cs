using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Common;

namespace Common {
    public static class QueryManager {

        public delegate V QueryProvider<R, V>(R request) where R : QueryRequest;

        private static readonly QueryManagerImplementation INTERNAL_MANAGER = new QueryManagerImplementation();

        /// <summary>
        /// Registers a provider
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="provider"></param>
        public static void RegisterProvider<R, V>(QueryProvider<R, V> provider) where R : QueryRequest {
            INTERNAL_MANAGER.RegisterProvider(provider);
        }

        /// <summary>
        /// Removes the provider with the specified request
        /// </summary>
        /// <typeparam name="R"></typeparam>
        public static void RemoveProvider<R>() {
            INTERNAL_MANAGER.RemoveProvider<R>();
        }

        /// <summary>
        /// Returns whether or not there's a provider for the specified request type
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        public static bool HasProvider<R>() where R : QueryRequest {
            return INTERNAL_MANAGER.HasProvider<R>();
        }

        /// <summary>
        /// Queries for a value for the specified request.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static V Query<R, V>(R request) where R : QueryRequest {
            return INTERNAL_MANAGER.Query<R, V>(request);
        }

    }
}
