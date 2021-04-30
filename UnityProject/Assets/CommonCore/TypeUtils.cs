using System;
using System.Reflection;

#nullable enable

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
            Type? type = Type.GetType(typeName);

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
                if (assembly == null) {
                    continue;
                }

                // See if that assembly defines the named type
                type = assembly.GetType(typeName);
                if(type != null) {
                    return type;
                }
            }

            // The type just couldn't be found...
            throw new Exception($"Type can't be found for \"{typeName}\".");
        }

        /**
		 * Returns whether or not the specified property can be rendered as a variable
		 */
        public static bool IsVariableProperty(PropertyInfo property) {
            return CanReadAndWrite(property);
        }

        public static bool CanReadAndWrite(PropertyInfo property) {
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

        public static void CopyProperties<T>(T source, T destination) {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; ++i) {
                if (CanReadAndWrite(properties[i])) {
                    Copy(properties[i], source, destination);
                }
            }
        }

        private static void Copy<T>(PropertyInfo property, T source, T destination) {
            object value = property.GetGetMethod(false).Invoke(source, EMPTY_PARAMETERS);
            property.GetSetMethod(false).Invoke(destination, new object[] { value });
        }

        public static ConstructorInfo ResolveEmptyConstructor(Type type) {
            ConstructorInfo[] constructors = type.GetConstructors();
            foreach(ConstructorInfo constructor in constructors) {
                // we only need the default constructor
                if(constructor.GetParameters().Length == 0) {
                    return constructor;
                }
            }

            throw new Exception("Can't resolve appropriate constructor");
        }

        /// <summary>
        /// Resolves a custom attribute from the specified PropertyInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T? GetCustomAttribute<T>(PropertyInfo property) where T : Attribute {
            Attribute attribute = Attribute.GetCustomAttribute(property, typeof(T));
            return attribute as T;
        }

        /// <summary>
        /// Instantiates an instance from a class name
        /// </summary>
        /// <param name="className"></param>
        /// <param name="data"></param>
        /// <param name="parentVariables"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Option<T> Instantiate<T>(ClassData data, NamedValueLibrary parentVariables) where T : class {
            Option<Type> type = TypeIdentifier.GetType(data.ClassName);
            Assertion.IsSome(type, data.ClassName);

            return type.MatchExplicit<InstantiateMatcher<T>, Option<T>>(new InstantiateMatcher<T>(data, parentVariables));
        }

        private readonly struct InstantiateMatcher<T> : IFuncOptionMatcher<Type, Option<T>> where T : class {
            private readonly ClassData data;
            private readonly NamedValueLibrary parentVariables;

            public InstantiateMatcher(ClassData data, NamedValueLibrary parentVariables) {
                this.data = data;
                this.parentVariables = parentVariables;
            }
            
            public Option<T> OnSome(Type type) {
                ConstructorInfo constructor = ResolveEmptyConstructor(type);
                T instance = (T) constructor.Invoke(EMPTY_PARAMETERS);
            
                // Inject variables
                NamedValueUtils.InjectNamedProperties(this.parentVariables, this.data.Variables, type, instance);
                
                return Option<T>.Some(instance);
            }

            public Option<T> OnNone() {
                return Option<T>.NONE;
            }
        }
    }
}
