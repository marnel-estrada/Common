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
namespace Common.Query {
	/**
	 * The center point of query system.
	 */
	public static class QuerySystem {

		private static readonly QuerySystemImplementation systemInstance = new QuerySystemImplementation();

		/**
		 * Registers a resolver
		 */
		public static void RegisterResolver(string queryId, QueryResultResolver resolver) {
			systemInstance.RegisterResolver(queryId, resolver);
		}

		/**
		 * Removes a resolver
		 */
		public static void RemoveResolver(string queryId) {
			systemInstance.RemoveResolver(queryId);
		}

		/**
		 * Requests for a query
		 */
		public static T Query<T>(string queryId) {
			return systemInstance.Query<T>(queryId);
		}

		/**
		 * Starts a query.
		 */
		public static IQueryRequest Start(string queryId) {
			return systemInstance.Start(queryId);
		}

		/**
		 * Completes the query returning the result
		 */
		public static T Complete<T>(IQueryRequest request) {
			return systemInstance.Complete<T>(request);
		}
		
		/**
		 * Returns whether or not the specified query id has a resolver
		 */
		public static bool HasResolver(string queryId) {
			return systemInstance.HasResolver(queryId);
		}

	}
}