using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Common {

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

        private readonly struct StoreTypeMatcher : IOptionMatcher<Type> {
            private readonly string typeName;
            private readonly Dictionary<string, Type> resolvedTypes;

            public StoreTypeMatcher(string typeName, Dictionary<string, Type> resolvedTypes) {
                this.typeName = typeName;
                this.resolvedTypes = resolvedTypes;
            }
            
            public void OnSome(Type type) {
                this.resolvedTypes[this.typeName] = type;
            }

            public void OnNone() {
            }
        }

        /// <summary>
        /// Resolves the type using a string
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Option<Type> ResolveType(string typeName) {
            // Check if it was already resolved before
            Option<Type> foundType = this.resolvedTypes.Find(typeName);
            if (foundType.IsSome) {
                // It was already resolved
                return foundType;
            }

            foundType = ResolveTypeFromAssemblies(typeName);
            
            // Stores the type if it was resolved
            foundType.Match(new StoreTypeMatcher(typeName, this.resolvedTypes));
            
            return foundType;
        }

        /// <summary>
        /// Resolves the type using a string
        /// Looks up from assemblies (this is slow)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private Option<Type> ResolveTypeFromAssemblies(string typeName) {
            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            Type type = Type.GetType(typeName);

            // If it worked, then we're done here
            if(type != null) {
                return Option<Type>.Some(type);
            }
            
            // Attempt to search for type on the loaded assemblies
            int currentLength = this.currentAssemblies.Length;
            for(int i = 0; i < currentLength; ++i) {
                type = this.currentAssemblies[i].GetType(typeName);
                if(type != null) {
                    return Option<Type>.Some(type);
                }
            }
            
            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            int loadedLength = this.loadedAssemblies.Length;
            for (int i = 0; i < loadedLength; ++i) {
                type = this.loadedAssemblies[i].GetType(typeName);
                if(type != null) {
                    return Option<Type>.Some(type);
                }
            }

            // Not resolved at all
            return Option<Type>.NONE;
        }

        private static readonly TypeIdentifier INSTANCE = new TypeIdentifier();
        
        /// <summary>
        /// Resolves the type using a string
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Option<Type> GetType(string typeName) {
            return INSTANCE.ResolveType(typeName);
        }

    }
}
