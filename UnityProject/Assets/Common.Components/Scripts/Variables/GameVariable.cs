using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Abstract base class for objects that can query a game variable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GameVariable<T> {

        private readonly string key;
        private bool valueResolved;
        private T value;

        public GameVariable(string key) {
            this.key = key;
            this.valueResolved = false;
        }

        public T Value {
            get {
                if (!this.valueResolved) {
                    // value is not yet resolved
                    this.value = ResolveValue(this.key);
                    this.valueResolved = true;
                }

                return this.value;
            }
        }

        // Resolves the value of the specified key
        protected abstract T ResolveValue(string key);

    }
}
