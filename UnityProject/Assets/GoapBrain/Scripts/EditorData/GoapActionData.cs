using System;
using System.Collections.Generic;
using UnityEngine;

using Common;

using Unity.Collections;

namespace GoapBrain {
    [Serializable]
    public class GoapActionData {
        [SerializeField]
        private string name;

        [SerializeField]
        private float cost;

        [SerializeField]
        private bool cancellable = true; // default to true

        [SerializeField]
        private string comment;

        [SerializeField]
        private bool editComment;

        [SerializeField]
        private bool showComment;

        [SerializeField]
        private bool enabled = true;

        [SerializeField]
        private List<ConditionData> preconditions = new List<ConditionData>();

        // Note here that we changed the convention for each action to only have one effect
        [SerializeField]
        private ConditionData? effect;

        [SerializeField]
        private List<ClassData> atomActions = new List<ClassData>();

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;
            }
        }

        public int ActionId {
            get {
                return new FixedString64Bytes(this.name).GetHashCode();
            }
        }

        public List<ConditionData> Preconditions {
            get {
                return this.preconditions;
            }
        }

        public List<ClassData> AtomActions {
            get {
                return this.atomActions;
            }
        }

        public float Cost {
            get {
                return this.cost;
            }

            set {
                this.cost = value;
            }
        }

        public string Comment {
            get {
                return this.comment;
            }

            set {
                this.comment = value;
            }
        }

        public bool EditComment {
            get {
                return this.editComment;
            }

            set {
                this.editComment = value;
            }
        }

        public bool ShowComment {
            get {
                return this.showComment;
            }

            set {
                this.showComment = value;
            }
        }

        public bool Enabled {
            get {
                return this.enabled;
            }

            set {
                this.enabled = value;
            }
        }

        public bool Cancellable {
            get {
                return this.cancellable;
            }

            set {
                this.cancellable = value;
            }
        }

        public ConditionData? Effect {
            get {
                return this.effect;
            }
            set {
                this.effect = value;
            }
        }

    }
}
