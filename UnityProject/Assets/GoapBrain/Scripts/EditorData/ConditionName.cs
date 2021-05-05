using System;
using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class ConditionName {
        // Name of the condition
        [SerializeField]
        private string name;

        // Whether or not rename mode is on
        [SerializeField]
        private bool renameMode; // Editor tracker

        [SerializeField]
        private string newName;

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;
            }
        }

        public bool RenameMode {
            get {
                return this.renameMode;
            }

            set {
                this.renameMode = value;
            }
        }

        public string NewName {
            get {
                return this.newName;
            }

            set {
                this.newName = value;
            }
        }
    }
}
