using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// Attribute for adding text hints to classes and member variables
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class TextHint : Attribute {

        // the text of the hint
        private readonly string text;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        public TextHint(string text) {
            this.text = text;
        }

        public string Text {
            get {
                return text;
            }
        }

    }
}
