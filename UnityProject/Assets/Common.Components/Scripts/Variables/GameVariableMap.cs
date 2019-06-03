using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Common;

namespace Common {
    /// <summary>
    /// A utility class that manages game variables instances of a certain type
    /// We needed this so we don't needlessly have multiple game variables instances for a certain key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GameVariableMap<T> {

        private readonly Func<string, GameVariable<T>> instanceCreator;

        private readonly Dictionary<string, GameVariable<T>> map = new Dictionary<string, GameVariable<T>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instanceCreator"></param>
        public GameVariableMap(Func<string, GameVariable<T>> instanceCreator) {
            this.instanceCreator = instanceCreator;
        }

        /// <summary>
        /// Returns the value for the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetValue(string key) {
            GameVariable<T> variable = this.map.Find(key);
            if(variable != null) {
                // Already instantiated before
                return variable.Value;
            }

            // No variable yet for the specified key
            // We create a new one
            variable = this.instanceCreator(key);
            this.map[key] = variable;

            return variable.Value;
        }

    }
}
