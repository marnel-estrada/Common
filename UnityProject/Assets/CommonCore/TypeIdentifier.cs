using UnityEngine;

namespace Common {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A utility class used for faster identification of a class from a string
    /// This is faster due to caching of arrays of Assembly and AssemblyNames
    /// </summary>
    public class TypeIdentifier {
        private readonly Assembly[] currentAssemblies;
        private readonly Assembly[] loadedAssemblies;
        
        private readonly Dictionary<string, Type> resolvedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Constructor
        /// </summary>
        private TypeIdentifier() {
            this.currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            // Load assemblies
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            this.loadedAssemblies = new Assembly[referencedAssemblies.Length];
            for (int i = 0; i < referencedAssemblies.Length; ++i) {
                // We try catch here so that the loading continues even if there are errors
                // The error was reported in Crashes and Exceptions from Unity
                try {
                    this.loadedAssemblies[i] = Assembly.Load(referencedAssemblies[i]);
                } catch (Exception e) {
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Resolves the type using a string
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Type ResolveType(string typeName) {
            // Check if it was already resolved before
            Type type = this.resolvedTypes.Find(typeName);
            if (type != null) {
                // It was already resolved
                return type;
            }

            type = ResolveTypeFromAssemblies(typeName);
            if (type != null) {
                // It was resolved. We store it so that we only lookup the dictionary next time
                this.resolvedTypes[typeName] = type;
            }
            
            return type;
        }

        /// <summary>
        /// Resolves the type using a string
        /// Looks up from assemblies (this is slow)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private Type ResolveTypeFromAssemblies(string typeName) {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            Type type = Type.GetType(typeName);

            // If it worked, then we're done here
            if(type != null) {
                return type;
            }
            
            // Attempt to search for type on the loaded assemblies
            int currentLength = this.currentAssemblies.Length;
            for(int i = 0; i < currentLength; ++i) {
                type = this.currentAssemblies[i].GetType(typeName);
                if(type != null) {
                    return type;
                }
            }
            
            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            int loadedLength = this.loadedAssemblies.Length;
            for (int i = 0; i < loadedLength; ++i) {
                type = this.loadedAssemblies[i].GetType(typeName);
                if(type != null) {
                    return type;
                }
            }

            return null;
        }

        private static readonly TypeIdentifier INSTANCE = new TypeIdentifier();
        
        /// <summary>
        /// Resolves the type using a string
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName) {
            return INSTANCE.ResolveType(typeName);
        }
    }
}
