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
		public Maybe<TimeReference> Get(string name) {
			if(string.IsNullOrEmpty(name)) {
				return new Maybe<TimeReference>(TimeReference.GetDefaultInstance());
			}

			// May return null if the time reference was not added yet
			TimeReference foundReference = this.instanceMap.Find(name);
			if (foundReference == null) {
				return Maybe<TimeReference>.Nothing;
			}
			
			return new Maybe<TimeReference>(foundReference);
		}
		
		/**
		 * Returns the default TimeReference instance.
		 */
		public TimeReference GetDefault() {
			return TimeReference.GetDefaultInstance();
		}
	}
}
