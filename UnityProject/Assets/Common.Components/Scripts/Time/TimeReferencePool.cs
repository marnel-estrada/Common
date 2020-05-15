using System;
using System.Collections.Generic;

namespace Common.Time {
	/**
	 * Class that manages multiple TimeReference instances.
	 */
	public class TimeReferencePool {
		private readonly Dictionary<string, TimeReference> instanceMap;
		
		private TimeReferencePool() {
			// can't be instantiated, this is a singleton
			this.instanceMap = new Dictionary<string, TimeReference>();
		}
		
		private static TimeReferencePool ONLY_INSTANCE = null;
		
		/**
		 * Returns the only TimeReferencePool instance.
		 */
		public static TimeReferencePool GetInstance() {
			if(ONLY_INSTANCE == null) {
				ONLY_INSTANCE = new TimeReferencePool();
			}
			
			return ONLY_INSTANCE;
		}
		
		/**
		 * Adds a TimeReference instance for the specified name.
		 */
		public TimeReference Add(string name) {
			TimeReference newTimeReference = new TimeReference(name);
			this.instanceMap[name] = newTimeReference;
			return newTimeReference;
		}
		
		/**
		 * Retrieves the TimeReference instance for this specified name.
		 */
		public TimeReference Get(string name) {
			if (this.instanceMap.TryGetValue(name, out TimeReference timeReference)) {
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
	}
}
