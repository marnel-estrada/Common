using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common {
    /// <summary>
    /// Contains a named map of every serializable type possible
    /// </summary>
    [Serializable]
    public class NamedValueLibrary {
        [SerializeField]
        private NamedStringMap stringMap = new NamedStringMap();

        [SerializeField]
        private NamedIntMap intMap = new NamedIntMap();

        [SerializeField]
        private NamedFloatMap floatMap = new NamedFloatMap();

        [SerializeField]
        private NamedBoolMap boolMap = new NamedBoolMap();

        [SerializeField]
        private NamedVector3Map vector3Map = new NamedVector3Map();

        [SerializeField]
        private NamedIntVector2Map intVector2Map = new NamedIntVector2Map();

        private Dictionary<NamedValueType, NamedValueContainer> containerMap;

        /// <summary>
        /// Constructor
        /// </summary>
        public NamedValueLibrary() {
            // populate container map
            this.containerMap = new Dictionary<NamedValueType, NamedValueContainer>();

            AddContainerMapping(NamedValueType.STRING, new NamedMapContainerWrapper<NamedString>(delegate() {
                return this.stringMap;
            }));

            AddContainerMapping(NamedValueType.INT, new NamedMapContainerWrapper<NamedInt>(delegate () {
                return this.intMap;
            }));

            AddContainerMapping(NamedValueType.FLOAT, new NamedMapContainerWrapper<NamedFloat>(delegate () {
                return this.floatMap;
            }));

            AddContainerMapping(NamedValueType.BOOL, new NamedMapContainerWrapper<NamedBool>(delegate () {
                return this.boolMap;
            }));

            AddContainerMapping(NamedValueType.VECTOR3, new NamedMapContainerWrapper<NamedVector3>(delegate () {
                return this.vector3Map;
            }));

            AddContainerMapping(NamedValueType.INT2, new NamedMapContainerWrapper<NamedInt2>(delegate() {
                return this.intVector2Map;
            }));
        }

        private void AddContainerMapping(NamedValueType type, NamedValueContainer container) {
            Assertion.Assert(!this.containerMap.ContainsKey(type)); // type should not have been added yet
            this.containerMap[type] = container;
        }

        /// <summary>
        /// Returns whether or not the specified named Type is supported
        /// </summary>
        /// <param name="namedType"></param>
        /// <returns></returns>
        public static bool IsSupported(Type namedType) {
            return NamedValueType.IsSupportedNamedType(namedType);
        }

        public NamedStringMap Strings {
            get {
                return stringMap;
            }
        }

        public NamedIntMap Integers {
            get {
                return this.intMap;
            }
        }

        public NamedFloatMap Floats {
            get {
                return this.floatMap;
            }
        }

        public NamedBoolMap Booleans {
            get {
                return this.boolMap;
            }
        }

        public NamedVector3Map Vector3s {
            get {
                return this.vector3Map;
            }
        }

        /// <summary>
        /// Returns the Named* instance with the specified name
        /// Note that its not the primitive value that's returned here
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object Get(string name, NamedValueType type) {
            Assertion.Assert(this.containerMap.TryGetValue(type, out NamedValueContainer container), type.Label);

            return container.Get(name);
        }

        /// <summary>
        /// Adds a variable for the specified type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void Add(string name, NamedValueType type) {
            NamedValueContainer container = null;
            Assertion.Assert(this.containerMap.TryGetValue(type, out container), type.Label);

            container.Add(name);
        }

        /// <summary>
        /// Returns whether or not the specified variable exists
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Contains(string name, NamedValueType type) {
            Option<NamedValueContainer> container = this.containerMap.Find(type);
            return container.Match<ContainsMatcher, bool>(new ContainsMatcher(name));
        }
        
        private readonly struct ContainsMatcher : IFuncOptionMatcher<NamedValueContainer, bool> {
            private readonly string name;

            public ContainsMatcher(string name) {
                this.name = name;
            }

            public bool OnSome(NamedValueContainer container) {
                return container.Contains(this.name);
            }

            public bool OnNone() {
                // No container
                return false;
            }
        }

        /// <summary>
        /// Copies the values of this library to the specified one
        /// </summary>
        /// <param name="otherLibrary"></param>
        public void CopyTo(NamedValueLibrary otherLibrary) {
            // clear other first
            foreach(KeyValuePair<NamedValueType, NamedValueContainer> entry in otherLibrary.containerMap) {
                entry.Value.Clear();
            }

            // copy each entry here to other
            foreach (KeyValuePair<NamedValueType, NamedValueContainer> entry in this.containerMap) {
                NamedValueType valueType = entry.Key;
                NamedValueContainer container = entry.Value;
                for(int i = 0; i < container.Count; ++i) {
                    Named named = (Named)container.GetAt(i);
                    otherLibrary.Add(named.Name, valueType);

                    ValueHolder entryFromOther = (ValueHolder)otherLibrary.Get(named.Name, valueType);
                    ValueHolder ourEntry = (ValueHolder)named; // note that NamedMap entries also implements ValueHolder
                    entryFromOther.Set(ourEntry.Get());
                    entryFromOther.UseOtherHolder = ourEntry.UseOtherHolder;
                    entryFromOther.OtherHolderName = ourEntry.OtherHolderName;
                }
            }
        }

        /// <summary>
        /// Adds the variables from the specified library
        /// </summary>
        /// <param name="otherLibrary"></param>
        public void Add(NamedValueLibrary otherLibrary) {
            foreach (KeyValuePair<NamedValueType, NamedValueContainer> entry in otherLibrary.containerMap) {
                NamedValueType valueType = entry.Key;
                NamedValueContainer container = entry.Value;
                for (int i = 0; i < container.Count; ++i) {
                    Named named = (Named)container.GetAt(i);

                    if(this.Contains(named.Name, valueType)) {
                        // Already exists
                        continue;
                    }

                    Add(named.Name, valueType);

                    ValueHolder entryFromOther = (ValueHolder)named;
                    ValueHolder ourEntry = Get(named.Name, valueType) as ValueHolder;
                    ourEntry.Set(entryFromOther.Get());
                    ourEntry.UseOtherHolder = entryFromOther.UseOtherHolder;
                    ourEntry.OtherHolderName = entryFromOther.OtherHolderName;
                }
            }
        }

        /// <summary>
        /// Returns the container for the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public NamedValueContainer GetContainer(NamedValueType type) {
            return this.containerMap[type];
        }
    }
}
