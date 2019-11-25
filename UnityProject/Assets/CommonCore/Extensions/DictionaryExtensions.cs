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
            dictionary.TryGetValue(key, out V value);

            // May return null so client code should check or it
            return value;
        }

        /// <summary>
        /// A more safe version of Find()
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static Option<V> FindAsOption<K, V>(this Dictionary<K, V> dictionary, K key) 
            where V : class {
            return dictionary.TryGetValue(key, out V value) ? Option<V>.Some(value) : Option<V>.NONE;
        }
    }
}
