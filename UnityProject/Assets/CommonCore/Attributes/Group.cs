using System;

namespace Common {
    /**
	 * An attribute used for grouping classes
	 */
    [AttributeUsage(AttributeTargets.Class)]
    public class Group : Attribute {
        /**
		 * Constructor
		 */
        public Group(string name) {
            this.Name = name;
        }

        public string Name { get; }
    }
}