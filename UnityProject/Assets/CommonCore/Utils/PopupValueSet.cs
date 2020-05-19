namespace Common {
	/**
	 * A utility class for handling a list of values that can be displayed in a popup.
	 */
	public class PopupValueSet {
        // we separate display and value because some fields expects an empty value but popup editor won't render it as selectable
		private string[] displayList;
        private string[] valueList;

        /// <summary>
        /// No list may be specified initially
        /// Update() might be invoked later
        /// </summary>
        public PopupValueSet() {
        }
		
        /**
         * Constructor
         * The specified display list will also be the value list
         */
        public PopupValueSet(string[] displayList) : this(displayList, displayList) {
        }

		/**
		 * Constructor with specified display list and value list
		 */
		public PopupValueSet(string[] displayList, string[] valueList) {
            this.displayList = displayList;
			this.valueList = valueList;
            Assertion.IsTrue(this.displayList.Length == this.valueList.Length); // they should have equal length
		}
		
		public string[] DisplayList {
			get {
				return this.displayList;
			}
		}
		
		/**
		 * Resolves for the index of the specified value in the set.
		 */
		public int ResolveIndex(string textValue) {
			for(int i = 0; i < valueList.Length; ++i) {
				if(valueList[i].Equals(textValue)) {
					return i;
				}
			}
			
			// Returns a negative to mark that no value was found
            // Client code should check for this
			return -1;
		}
		
		/**
		 * Returns the value at the specified index.
		 */
		public string GetValue(int index) {
			return this.valueList[index];
		}

        /// <summary>
        /// Updates the display and value list
        /// </summary>
        /// <param name="displayList"></param>
        /// <param name="valueList"></param>
        public void Update(string[] displayList, string[] valueList) {
            this.displayList = displayList;
            this.valueList = valueList;
        }
	}
}

