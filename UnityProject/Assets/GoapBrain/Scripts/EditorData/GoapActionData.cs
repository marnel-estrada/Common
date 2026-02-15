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
        private List<ConditionData> preconditions = new();

        // Note here that we changed the convention for each action to only have one effect
        [SerializeField]
        private ConditionData? effect;

        [SerializeField]
        private List<ClassData> atomActions = new();

        public string Name {
            get => this.name;
            set => this.name = value;
        }

        public int ActionId => new FixedString64Bytes(this.name).GetHashCode();

        public List<ConditionData> Preconditions => this.preconditions;

        public List<ClassData> AtomActions => this.atomActions;

        public float Cost {
            get => this.cost;

            set => this.cost = value;
        }

        public string Comment {
            get => this.comment;

            set => this.comment = value;
        }

        public bool EditComment {
            get => this.editComment;

            set => this.editComment = value;
        }

        public bool ShowComment {
            get => this.showComment;

            set => this.showComment = value;
        }

        public bool Enabled {
            get => this.enabled;

            set => this.enabled = value;
        }

        public bool Cancellable {
            get => this.cancellable;

            set => this.cancellable = value;
        }

        public ConditionData? Effect {
            get => this.effect;
            set => this.effect = value;
        }

        public GoapActionData CreateCopy() {
            // Copy preconditions
            List<ConditionData> preconditionsCopy = new(this.preconditions.Count);
            foreach (ConditionData conditionData in this.preconditions) {
                preconditionsCopy.Add(conditionData.CreateCopy());
            }
            
            // Copy atom actions
            List<ClassData> atomActionsCopy = new(this.atomActions.Count);
            foreach (ClassData atomActionData in this.atomActions) {
                ClassData atomActionDataCopy = new();
                atomActionData.CopyTo(atomActionDataCopy);
                atomActionsCopy.Add(atomActionDataCopy);
            }
            
            return new GoapActionData() {
                name = this.name,
                cost = this.cost,
                cancellable = this.cancellable,
                comment = this.comment,
                editComment = this.editComment,
                showComment = this.showComment,
                enabled = this.enabled,
                preconditions = preconditionsCopy,
                effect = this.effect?.CreateCopy(),
                atomActions = atomActionsCopy
            };
        }
    }
}
