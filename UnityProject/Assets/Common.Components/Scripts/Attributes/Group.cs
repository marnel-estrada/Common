using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /**
	 * An attribute used for grouping classes
	 */
    [AttributeUsage(AttributeTargets.Class)]
    public class Group : Attribute {

        private readonly string name;

        /**
		 * Constructor
		 */
        public Group(string name) {
            this.name = name;
        }

        public string Name {
            get {
                return name;
            }
        }

    }
}
