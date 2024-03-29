﻿using System;

using UnityEngine;

namespace Common {
    /// <summary>
    /// A base class for serialized pair of key and value
    /// The usage is to create a subclass from this one and use that class as serialized field in your own MonoBehaviour
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    [Serializable]
    public abstract class SerializedKeyValue<K, V> {

        [SerializeField]
        private K key;

        [SerializeField]
        private V value;

        public K Key {
            get {
                return this.key;
            }

            set {
                this.key = value;
            }
        }

        public V Value {
            get {
                return this.value;
            }

            set {
                this.value = value;
            }
        }

    }
}
