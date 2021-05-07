using System;

using System.Reflection;

namespace Common {
    public static class NamedValueUtils {
        private static readonly CachedTypeProperties CACHED_PROPERTIES = new CachedTypeProperties(BindingFlags.Public | BindingFlags.Instance);
        
        /// <summary>
        /// Injects the Named* properties from the specified variables
        /// </summary>
        /// <param name="parentVariables"></param>
        /// <param name="localVariables"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        public static void InjectNamedProperties(NamedValueLibrary? parentVariables, NamedValueLibrary localVariables, Type type, object instance) {
            PropertyInfo[] properties = CACHED_PROPERTIES.GetProperties(type);
            foreach (PropertyInfo property in properties) {
                if (!TypeUtils.IsVariableProperty(property)) {
                    continue;
                }

                if (!NamedValueLibrary.IsSupported(property.PropertyType)) {
                    // not a supported type
                    continue;
                }
                
                NamedValueType namedType = NamedValueType.ConvertFromPropertyType(property.PropertyType);
                NamedValueHolder variable = localVariables.Get(property.Name, namedType) as NamedValueHolder;

                if(variable == null) {
                    // Not found. We temporarily add the variable so that we don't get into problems.
                    // This usually happens when a new variable is introduced to the class but is not loaded
                    // in editor so that variable was not added.
                    localVariables.Add(property.Name, namedType);
                    variable = localVariables.Get(property.Name, namedType) as NamedValueHolder;
                }

                Assertion.NotNull(variable, property.Name);

                // resolve the NamedVariable instace to set to the property
                object namedInstance = variable;

                // check if variable is referring to one of event's variable
                if (variable.UseOtherHolder && parentVariables != null) {
                    // Uses parent. Get a reference of a Named* variable from the parent
                    namedInstance = parentVariables.Get(variable.OtherHolderName, namedType);
                } else {
                    // Does not use parent. We make a copy because the source may be from a template object like ScriptableObject
                    namedInstance = localVariables.GetContainer(namedType).GetCopy(property.Name);

                    // We clear these names as they will no longer be used
                    // This is to save memory
                    NamedValueHolder namedInstanceAsValueHolder = namedInstance as NamedValueHolder;
                    namedInstanceAsValueHolder?.ClearName();
                    namedInstanceAsValueHolder?.ClearOtherHolderName();
                }

                // Finally assign to the property
                // Note here that the property is Named* property, not primitive types
                property.SetValue(instance, namedInstance, null);
            }
        }
    }
}
