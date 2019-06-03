using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// Common hints that UI editors could use
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorHint : Attribute {

        // possible hints that can be used
        public const string SELECTION = "Selection";
        public const string HIDDEN = "Hidden";

        private readonly string hint;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hint"></param>
        public EditorHint(string hint) {
            this.hint = hint;
        }

        public string Hint {
            get {
                return hint;
            }
        }

    }
}
