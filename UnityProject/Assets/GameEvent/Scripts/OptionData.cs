using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GameEvent {
    [Serializable]
    public class OptionData : Identifiable, IntIdentifiable, IDuplicable<OptionData> {
        [SerializeField]
        private int id;

        [SerializeField]
        private string nameId;
        
        [SerializeField]
        private string descriptionId;

        // A comment that can be shown in editor
        [SerializeField]
        private string comment;
        
        [SerializeField]
        private int childEventId = -1;

        // We use integer here so it's easier when making the editor
        [SerializeField]
        private int trueOutcomeProbability = 100;

        [SerializeField]
        private string trueOutcomeTextId;

        [SerializeField]
        private string falseOutcomeTextId;

        [SerializeField]
        private List<ClassData> requirements = new List<ClassData>();

        [SerializeField]
        private List<ClassData> costs = new List<ClassData>();

        [SerializeField]
        private List<ClassData> effects = new List<ClassData>();

        public OptionData(int id) {
            this.id = id;
        }

        [PropertyGroup("Id")]
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

        [Hidden]
        public int ChildEventId {
            get {
                return this.childEventId;
            }
            set {
                this.childEventId = value;
            }
        }

        public bool HasChildEvent {
            get {
                return this.childEventId >= 0;
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

        [PropertyRenderer("Common.ProbabilityRenderer")]
        public int TrueOutcomeProbability {
            get {
                return this.trueOutcomeProbability;
            }
            set {
                this.trueOutcomeProbability = value;
            }
        }

        [PropertyGroup("Outcome Text")]
        public string TrueOutcomeTextId {
            get {
                return this.trueOutcomeTextId;
            }
            set {
                this.trueOutcomeTextId = value;
            }
        }

        [PropertyGroup("Outcome Text")]
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

        public string Id {
            get {
                return this.nameId;
            }

            set {
                this.nameId = value;
            }
        }

        [PropertyGroup("Id")]
        [ReadOnlyField]
        public int IntId {
            get {
                return this.id;
            }

            set {
                this.id = value;
            }
        }

        [PropertyRenderer("GameEvent.CommentRenderer")]
        public string Comment {
            get {
                return this.comment;
            }
            set {
                this.comment = value;
            }
        }

        public OptionData Duplicate() {
            OptionData copy = new OptionData(this.id);
            TypeUtils.CopyProperties(this, copy);
            
            // Copy requirements
            copy.requirements = new List<ClassData>(this.requirements.Count);
            ClassData.Copy(this.requirements, copy.requirements);
            
            // Copy costs
            copy.costs = new List<ClassData>(this.costs.Count);
            ClassData.Copy(this.costs, copy.costs);
            
            // Copy effects
            copy.effects = new List<ClassData>(this.effects.Count);
            ClassData.Copy(this.effects, copy.effects);

            return copy;
        }
    }
}