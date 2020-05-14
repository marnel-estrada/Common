using System;
using System.Collections.Generic;
using System.Reflection;

namespace Common {
    /// <summary>
    /// Caches mapping of Type to its PropertyInfo[]. This is done to avoid garbage.
    /// </summary>
    public class CachedTypeProperties {
        private readonly Dictionary<Type, PropertyInfo[]> map = new Dictionary<Type, PropertyInfo[]>(50);
        private readonly BindingFlags flags;

        // Set as private as this will be a singleton
        public CachedTypeProperties(BindingFlags flags) {
            this.flags = flags;
        }
        
        public PropertyInfo[] GetProperties(Type type) {
            if (this.map.TryGetValue(type, out PropertyInfo[] properties)) {
                // Already exists
                return properties;
            }
            
            // Not yet in the map. We cache it.
            properties = type.GetProperties(this.flags);
            this.map[type] = properties;

            return properties;
        }
    }
}