namespace Common {
	/**
	 * Common interface for named objects
	 */
	public interface Named {
		/**
		 * Returns a name
		 */
		string Name {get; set;}

        /// <summary>
        /// Clears the name
        /// This is used to save memory. Sometimes the names is no longer needed.
        /// </summary>
        void ClearName();
	}
}

