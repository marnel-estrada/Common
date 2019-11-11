using System.Collections.Generic;

namespace Common {
    /// <summary>
    /// Contains extensions to the Dictionary class
    /// </summary>
    public static class DictionaryExtensions {
        /// <summary>
        /// Looks for the value of the specified key. Returns default value if none was found.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static V Find<K, V>(this Dictionary<K, V> dictionary, K key) {
            V value = default(V);
            dictionary.TryGetValue(key, out value);

            // May return null so client code should check or it
            return value;
        }
    }
}
