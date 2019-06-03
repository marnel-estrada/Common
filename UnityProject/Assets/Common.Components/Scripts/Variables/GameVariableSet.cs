using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    class GameVariableSet {

        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public GameVariableSet() {
        }

        /// <summary>
        /// Adds an entry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value) {
            // Should have no duplicate entry
            Assertion.Assert(!this.variables.ContainsKey(key), "Duplicate entry: " + key);
            this.variables[key] = value;
        }

        /// <summary>
        /// Sets a value to the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value) {
            Assertion.Assert(Contains(key)); // Should contain the key
            this.variables[key] = value;
        }

        /// <summary>
        /// Returns the value of the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) {
            return this.variables[key];
        }

        /// <summary>
        /// Returns whether or not the set contains the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key) {
            return this.variables.ContainsKey(key);
        }

        public int Count {
            get {
                return this.variables.Count;
            }
        }

    }
}
