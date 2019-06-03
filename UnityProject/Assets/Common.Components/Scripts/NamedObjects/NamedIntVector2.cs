using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Math;

namespace Common {
    [Serializable]
    public class NamedIntVector2 : NamedValue<IntVector2> {

        /// <summary>
        /// Constructor
        /// </summary>
        public NamedIntVector2() : base() {
            // Make sure that value is instantiated
            this.VariableInstance = new IntVector2();
        }

        public override IntVector2 Value {
            get {
                return base.Value;
            }

            set {
                // Note here that we just copy the values
                // We don't replace the pointer
                base.Value.Set(value);
            }
        }

    }
}
