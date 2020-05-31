//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Common;

namespace Common.Query {
	/**
	 * An instantiated implementation of the query system.
	 */
	class QuerySystemImplementation {

		private Dictionary<string, QueryResultResolver> resolverMap;

        private int pendingRequestCount = 0;

		/**
		 * Constructor
		 */
		public QuerySystemImplementation() {
			this.resolverMap = new Dictionary<string, QueryResultResolver>();
		}

		/**
		 * Registers a query resolver
		 */
		public void RegisterResolver(string queryId, QueryResultResolver resolver) {
			Assertion.Assert(!this.resolverMap.ContainsKey(queryId)); // system should not contains the specified resolver yet
			this.resolverMap[queryId] = resolver;
		}

		/**
		 * Removes the specified resolver
		 */
		public void RemoveResolver(string queryId) {
			this.resolverMap.Remove(queryId);
		}

        private IQueryRequest quickRequest = new ConcreteQueryRequest();

		/**
		 * Requests for a query
		 */
		public T Query<T>(string queryId) {
            QueryResultResolver resolver = this.resolverMap.Find(queryId);
            Assertion.AssertNotNull(resolver);
		    return (T)resolver(this.quickRequest);
		}

        private Pool<ConcreteQueryRequest> requestPool = new Pool<ConcreteQueryRequest>();
        private const int PENDING_REQUEST_LIMIT = 100;

		/**
		 * Starts a query.
		 */
		public IQueryRequest Start(string queryId) {
            // If pending request count has reached the limit, then it appears that me may have an infinite query under another query
            // Or someone is not completing the request
			Assertion.Assert(this.pendingRequestCount <= PENDING_REQUEST_LIMIT);
			Assertion.Assert(this.resolverMap.ContainsKey(queryId)); // system should contain a resolver for the specified id

            // Initialize a recycled request
            ConcreteQueryRequest request = this.requestPool.Request();
            request.Clear();
            request.QueryId = queryId;
            
            ++this.pendingRequestCount;

			return request;
		}

		/**
		 * Completes the query returning the result
		 */
		public T Complete<T>(IQueryRequest request) {
			Assertion.Assert(this.pendingRequestCount > 0); // it should be locked at this point

			try {
				return (T)this.resolverMap[request.QueryId](request);
			} finally {
                --this.pendingRequestCount;
                this.requestPool.Recycle(request as ConcreteQueryRequest);
			}
		}
		
		/**
		 * Returns whether or not the query id has a resolver
		 */
		public bool HasResolver(string queryId) {
			return this.resolverMap.ContainsKey(queryId);
		}

	}
}