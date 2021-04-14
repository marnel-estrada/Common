using System;

using Common;

using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class ConditionData {
        [SerializeField]
        private string name;

        [SerializeField]
        private ConditionId id;

        [SerializeField]
        private bool value;

        /// <summary>
        ///     Empty constructor
        /// </summary>
        public ConditionData() {
        }

        /// <summary>
        ///     Constructor with specified values
        /// </summary>
        /// <param name="name"></param>
        public ConditionData(string name, bool value) {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        ///     A condition that uses an ID instead of a string name
        ///     This is used to reduce memory usage
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public ConditionData(ConditionId id, bool value) {
            this.id = id;
            this.value = value;
        }

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;
            }
        }

        public bool Value {
            get {
                return this.value;
            }

            set {
                this.value = value;
            }
        }

        public ConditionId Id {
            get {
                return this.id;
            }
        }

        /// <summary>
        ///     Sets an ID
        ///     We did it this way so we can easily search which parts are explicitly setting the ID
        /// </summary>
        /// <param name="id"></param>
        public void SetId(ConditionId id) {
            this.id = id;

            // If string name is present, they should have the same name with the associated ID
            if (!string.IsNullOrEmpty(this.name)) {
                Assertion.IsTrue(this.name == ConditionNamesDatabase.Instance.GetName(this.id));
            }
        }
    }
}