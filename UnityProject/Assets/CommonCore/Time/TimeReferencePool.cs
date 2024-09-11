using System;
using System.Collections.Generic;

namespace Common.Time {
	/**
	 * Class that manages multiple TimeReference instances.
	 */
	public class TimeReferencePool {
		// Note here that we use int for the key so that we can query by integer when the TimeReference
		// is from ECS which uses int as ID.
		private readonly Dictionary<int, TimeReference> instanceMap = new(2);
		
		private static readonly TimeReferencePool ONLY_INSTANCE = new();
		
		/**
		 * Returns the only TimeReferencePool instance.
		 */
		public static TimeReferencePool GetInstance() {
			return ONLY_INSTANCE;
		}
		
		/**
		 * Adds a TimeReference instance for the specified name.
		 */
		public void Add(int id) {
			TimeReference newTimeReference = new(id);
			this.instanceMap[id] = newTimeReference;
		}
		
		/**
		 * Retrieves the TimeReference instance for this specified name.
		 */
		public TimeReference Get(int id) {
			if (this.instanceMap.TryGetValue(id, out TimeReference timeReference)) {
				return timeReference;
			}
			
			throw new Exception($"Can't find TimeReference named \"name\"");
		}
		
		/**
		 * Returns the default TimeReference instance.
		 */
		public TimeReference GetDefault() {
			return TimeReference.GetDefaultInstance();
		}

		/// <summary>
		/// Used for domain reload.
		/// </summary>
		public void Reset() {
			this.instanceMap.Clear();
		}
	}
}
