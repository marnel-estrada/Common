using System;
using System.Reflection;

using UnityEngine;

namespace Common {
    public class PublicStaticFieldsInvoker {
        private readonly Type parentType;
        private readonly FieldInfo[] fields;
        
        // The methods to call
        private readonly SimpleList<string> methodNames = new SimpleList<string>(1);

        // This ensures that there's always at least one method name
        public PublicStaticFieldsInvoker(Type parentType, string methodName) {
            this.parentType = parentType;
            this.fields = this.parentType.GetFields(BindingFlags.Public | BindingFlags.Static);
            
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
            foreach (FieldInfo field in this.fields) {
                // The type of the field does not matter. As long it has the method
                // ClearProvider(), it will be invoked
                object instance = field.GetValue(null);
                MethodInfo clearMethod = field.FieldType.GetMethod(methodName);
                if (clearMethod != null) {
                    clearMethod.Invoke(instance, EMPTY_PARAMETERS);
                }
            }
        }
    }
}
