using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Common {
    /// <summary>
    /// Contains type related utility methods
    /// </summary>
    public static class TypeUtils {

        /// <summary>
        /// This is used to instantiate classes with empty constructors using reflection
        /// </summary>
        public static readonly object[] EMPTY_PARAMETERS = new object[0];

        /**
		 * A generic method for getting a type. Type.GetType() does not work for dynamic assemblies.
		 * This code is taken from http://answers.unity3d.com/questions/206665/typegettypestring-does-not-work-in-unity.html.
		 */
        public static Type GetType(string typeName) {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            Type type = Type.GetType(typeName);

            // If it worked, then we're done here
            if(type != null) {
                return type;
            }

            // Attempt to search for type on the loaded assemblies
            Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in currentAssemblies) {
                type = assembly.GetType(typeName);
                if(type != null) {
                    return type;
                }
            }

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach(AssemblyName assemblyName in referencedAssemblies) {
                // Load the referenced assembly
                Assembly assembly = Assembly.Load(assemblyName);
                if(assembly != null) {
                    // See if that assembly defines the named type
                    type = assembly.GetType(typeName);
                    if(type != null) {
                        return type;
                    }
                }
            }

            // The type just couldn't be found...
            return null;
        }

        /**
		 * Returns whether or not the specified property can be rendered as a variable
		 */
        public static bool IsVariableProperty(PropertyInfo property) {
            // should be writable and readable
            if(!(property.CanRead && property.CanWrite)) {
                return false;
            }

            // methods should be public
            MethodInfo getMethod = property.GetGetMethod(false);
            if(getMethod == null) {
                return false;
            }

            MethodInfo setMethod = property.GetSetMethod(false);
            if(setMethod == null) {
                return false;
            }

            return true;
        }

        public static ConstructorInfo ResolveEmptyConstructor(Type type) {
            ConstructorInfo[] constructors = type.GetConstructors();
            foreach(ConstructorInfo constructor in constructors) {
                // we only need the default constructor
                if(constructor.GetParameters().Length == 0) {
                    return constructor;
                }
            }

            Assertion.Assert(false, "Can't resolve appropriate constructor");
            return null;
        }

        /// <summary>
        /// Resolves a custom attribute from the specified PropertyInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(PropertyInfo property) where T : Attribute {
            Attribute attribute = Attribute.GetCustomAttribute(property, typeof(T));
            return attribute as T;
        }

    }
}
