using System;
using Unity.Mathematics;

namespace Common {
    [Serializable]
    public class NamedInt3 : NamedValue<int3> {
        public NamedInt3() {
            this.VariableInstance = new int3();
        }
    }
}