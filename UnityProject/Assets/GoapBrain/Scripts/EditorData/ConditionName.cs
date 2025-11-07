using System;
using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class ConditionName : IComparable<ConditionName> {
        // Name of the condition
        [SerializeField]
        private string name;

        // Whether or not rename mode is on
        [SerializeField]
        private bool renameMode; // Editor tracker

        [SerializeField]
        private string newName;

        public string Name {
            get => this.name;
            set => this.name = value;
        }

        public bool RenameMode {
            get => this.renameMode;
            set => this.renameMode = value;
        }

        public string NewName {
            get => this.newName;
            set => this.newName = value;
        }

        public int CompareTo(ConditionName? other) {
            if (ReferenceEquals(this, other)) {
                return 0;
            }

            if (other is null) {
                return 1;
            }

            return string.Compare(this.name, other.name, StringComparison.Ordinal);
        }
    }
}
