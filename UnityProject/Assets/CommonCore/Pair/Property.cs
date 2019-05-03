using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// A generic pair class that represents a property
    /// </summary>
    [Serializable]
    public class Property : SerializedKeyValue<string, string> {

        /// <summary>
        /// Default constructor
        /// </summary>
        public Property() {
        }

        /// <summary>
        /// Constructor with specified key and value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public Property(string key, string value) {
            this.Key = key;
            this.Value = value;
        }

    }
}
