using System;
using System.Collections.Generic;
using UnityEngine;

using Common;

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
        private List<Condition> preconditions = new List<Condition>();

        // Note here that we changed the convention for each action to only have one effect
        [SerializeField]
        private Condition? effect;

        [SerializeField]
        private List<ClassData> atomActions = new List<ClassData>();

        public string Name {
            get {
                return name;
            }

            set {
                this.name = value;
            }
        }

        public List<Condition> Preconditions {
            get {
                return preconditions;
            }
        }

        public List<ClassData> AtomActions {
            get {
                return atomActions;
            }
        }

        public float Cost {
            get {
                return cost;
            }

            set {
                this.cost = value;
            }
        }

        public string Comment {
            get {
                return comment;
            }

            set {
                this.comment = value;
            }
        }

        public bool EditComment {
            get {
                return editComment;
            }

            set {
                this.editComment = value;
            }
        }

        public bool ShowComment {
            get {
                return showComment;
            }

            set {
                this.showComment = value;
            }
        }

        public bool Enabled {
            get {
                return enabled;
            }

            set {
                this.enabled = value;
            }
        }

        public bool Cancellable {
            get {
                return cancellable;
            }

            set {
                cancellable = value;
            }
        }

        public Condition? Effect {
            get {
                return this.effect;
            }
            set {
                this.effect = value;
            }
        }

    }
}
