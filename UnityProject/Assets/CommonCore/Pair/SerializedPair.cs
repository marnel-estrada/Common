using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    /// <summary>
    /// A base class for serialized pair of objects
    /// The usage is to create a subclass from this one and use that class as serialized field in your own MonoBehaviour
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    [Serializable]
    public abstract class SerializedPair<T, U> {

        [SerializeField]
        private T first;

        [SerializeField]
        private U second;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SerializedPair() {
        }

        /// <summary>
        /// Constructor with specified value
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public SerializedPair(T first, U second) {
            this.first = first;
            this.second = second;
        }

        public T First {
            get {
                return this.first;
            }

            set {
                this.first = value;
            }
        }

        public U Second {
            get {
                return this.second;
            }

            set {
                this.second = value;
            }
        }

    }
}
