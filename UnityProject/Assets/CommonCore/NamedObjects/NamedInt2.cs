using System;

using Unity.Mathematics;

namespace Common {
    [Serializable]
    public class NamedInt2 : NamedValue<int2> {
        public NamedInt2() {
            // Make sure that value is instantiated
            this.VariableInstance = new int2();
        }
    }
}
