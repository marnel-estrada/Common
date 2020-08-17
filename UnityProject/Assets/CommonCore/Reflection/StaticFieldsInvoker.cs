using System;
using System.Reflection;

namespace Common {
    public class StaticFieldsInvoker {
        private readonly Type ownerType;
        private readonly FieldInfo[] fields;
        
        // The methods to call
        private readonly SimpleList<string> methodNames = new SimpleList<string>(1);

        // This ensures that there's always at least one method name
        public StaticFieldsInvoker(Type ownerType, string methodName) {
            this.ownerType = ownerType;
            this.fields = this.ownerType.GetFields(BindingFlags.Public | BindingFlags.Static);
            
            AddMethod(methodName);
        }

        public void AddMethod(string methodName) {
            this.methodNames.Add(methodName);
        }

        /// <summary>
        /// Calls the methods for each field
        /// </summary>
        public void Execute() {
            for (int i = 0; i < this.methodNames.Count; ++i) {
                Execute(this.methodNames[i]);
            }
        }
        
        private static readonly object[] EMPTY_PARAMETERS = new object[0];

        private void Execute(string methodName) {
            for (int i = 0; i < this.fields.Length; ++i) {
                FieldInfo field = this.fields[i];
                
                // The type of the field does not matter. As long it has the named method,
                // it will be invoked
                object instance = field.GetValue(null);
                MethodInfo clearMethod = field.FieldType.GetMethod(methodName);
                if (clearMethod != null) {
                    clearMethod.Invoke(instance, EMPTY_PARAMETERS);
                }
            }
        }
    }
}
