using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GameEvent {
    [Serializable]
    public class OptionData {
        private readonly int id;

        [SerializeField]
        private string nameId;
        
        [SerializeField]
        private string descriptionId;
        
        [SerializeField]
        private int childEventId;

        [SerializeField]
        private float trueOutcomeProbability;

        [SerializeField]
        private string trueOutcomeTextId;

        [SerializeField]
        private string falseOutcomeTextId;

        [SerializeField]
        private List<ClassData> requirements;

        [SerializeField]
        private List<ClassData> costs;

        [SerializeField]
        private List<ClassData> effects;

        public OptionData(int id) {
            this.id = id;
        }

        public string NameId {
            get {
                return this.nameId;
            }
            set {
                this.nameId = value;
            }
        }

        public string DescriptionId {
            get {
                return this.descriptionId;
            }
            set {
                this.descriptionId = value;
            }
        }

        public int ChildEventId {
            get {
                return this.childEventId;
            }
            set {
                this.childEventId = value;
            }
        }

        public List<ClassData> Requirements {
            get {
                return this.requirements;
            }
        }

        public List<ClassData> Costs {
            get {
                return this.costs;
            }
        }

        public float TrueOutcomeProbability {
            get {
                return this.trueOutcomeProbability;
            }
            set {
                this.trueOutcomeProbability = value;
            }
        }

        public string TrueOutcomeTextId {
            get {
                return this.trueOutcomeTextId;
            }
            set {
                this.trueOutcomeTextId = value;
            }
        }

        public string FalseOutcomeTextId {
            get {
                return this.falseOutcomeTextId;
            }
            set {
                this.falseOutcomeTextId = value;
            }
        }

        public List<ClassData> Effects {
            get {
                return this.effects;
            }
        }
    }
}