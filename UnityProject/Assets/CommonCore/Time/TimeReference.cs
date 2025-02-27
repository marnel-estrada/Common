namespace Common.Time {
	/**
	 * A class representing a single time frame of reference.
	 * This is used for instances where parts of the program need to run in normal time, but other parts need to run in different time scales.
	 * For example, in a speed up tower defense game.
	 */
	public class TimeReference {		
		private readonly int id;
		private float timeScale = 1.0f;
		
		/**
		 * Constructor
		 */
		public TimeReference(int id) {
			this.id = id;
		}
		
		/**
		 * Time scale property.
		 */
		public float TimeScale {
			get => this.timeScale;

			set => this.timeScale = value;
		}
		
		/**
		 * Returns the delta time for this time reference.
		 */
		public float DeltaTime => UnityEngine.Time.deltaTime * this.timeScale;

		public int Id => this.id;

		private static TimeReference? DEFAULT_INSTANCE;
		
		/**
		 * Returns a default instance that can be used by any class.
		 */
		public static TimeReference GetDefaultInstance() {
			return DEFAULT_INSTANCE ??= new TimeReference(0);
		}
	}
}
