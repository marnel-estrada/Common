using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// A utility class that caches an float game variable
    /// </summary>
    public class FloatGameVariable : GameVariable<float> {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public FloatGameVariable(string key) : base(key) {
        }

        protected override float ResolveValue(string key) {
            return GameVariablesQuery.GetFloat(key);
        }

    }
}
