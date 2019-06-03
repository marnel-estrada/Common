using System;

namespace Common.Time {
	/**
	 * A class representing a single time frame of reference.
	 * This is used for instances where parts of the program need to run in normal time, but other parts need to run in different time scales.
	 * For example, in a speed up tower defense game.
	 */
	public class TimeReference {		
		private string name;
		private float timeScale;
		
		/**
		 * Constructor
		 */
		public TimeReference(string name) {
			this.name = name;
		}
		
		/**
		 * Returns the name.
		 */
		public String Name {
			get {
				return name;
			}
		}
		
		/**
		 * Time scale property.
		 */
		public float TimeScale {
			get {
                return this.timeScale;
			}
			
			set {
                this.timeScale = value;
			}
		}
		
		/**
		 * Returns the delta time for this time reference.
		 */
		public float DeltaTime {
			get {
                return UnityEngine.Time.deltaTime * this.timeScale;
			}
		}
		
		private static TimeReference DEFAULT_INSTANCE;
		
		/**
		 * Returns a default instance that can be used by any class.
		 */
		public static TimeReference GetDefaultInstance() {
			if(DEFAULT_INSTANCE == null) {
				DEFAULT_INSTANCE = new TimeReference("RootTimeline");
			}
			
			return DEFAULT_INSTANCE;
		}
	}
}
