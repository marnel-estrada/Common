using System;
using System.Reflection;

namespace Common {
    public class StaticFieldsInvoker {
        private readonly Type ownerType;
        private readonly FieldInfo[] staticFields;
        
        // The methods to call
        private readonly SimpleList<string> methodNames = new(1);

        // This ensures that there's always at least one method name
        public StaticFieldsInvoker(Type ownerType, string methodName) {
            this.ownerType = ownerType;
            this.staticFields = this.ownerType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            
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
            for (int i = 0; i < this.staticFields.Length; ++i) {
                FieldInfo field = this.staticFields[i];
                
                // The type of the field does not matter. As long it has the named method,
                // it will be invoked
                object instance = field.GetValue(null);
                MethodInfo? method = field.FieldType.GetMethod(methodName);
                method?.Invoke(instance, EMPTY_PARAMETERS);
            }
        }
    }
}
