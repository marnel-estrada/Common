//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18449
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

namespace Common {
	/**
	 * A generic pool class.
	 */
	public class Pool<T> where T : class, new() {

		private readonly SimpleList<T> recycledItems; // list of unused instances

		/**
		 * Constructor
		 */
		public Pool() {
			this.recycledItems = new SimpleList<T>(10);
		}
		
		private static readonly object SYNC_LOCK = new object();

		/**
		 * Requests for an instance
		 */
		public T Request() {
			lock (SYNC_LOCK) {
				// check from pooledList
				if (this.recycledItems.IsEmpty()) {
					// create a new one
					return new T();
				}

				// get from pool
				// get from end of the list
				T instance = this.recycledItems[this.recycledItems.Count - 1];
				this.recycledItems.RemoveAt(this.recycledItems.Count - 1);

				return instance;
			}
		}
		

		/**
		 * Returns the instance to the pool
		 */
		public void Recycle(T instance) {
			lock (SYNC_LOCK) {
				Assertion.AssertNotNull(instance);

				// The item to be recycled should not be present in recycled items
				Assertion.Assert(!this.recycledItems.Contains(instance));

				if (this.recycledItems.Contains(instance)) {
					// Don't add if it was already recycled before
					// Note that execution just continues in production
					return;
				}

				this.recycledItems.Add(instance);
			}
		}

        /// <summary>
        /// Returns the current number of recycled instances
        /// </summary>
        public int Count {
            get {
                return this.recycledItems.Count;
            }
        }

	}
}
