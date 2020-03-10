using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GameEvent {
    [Serializable]
    public class EventData : IDataPoolItem, IDuplicable<EventData> {
        [SerializeField]
        private int id;
        
        [SerializeField]
        private string nameId;
        
        [SerializeField]
        private string descriptionId;
        
        [SerializeField]
        private string iconId;
        
        [SerializeField]
        private bool recurring;
        
        // We added this so we can disable some events
        [SerializeField]
        private bool enabled = true; // enabled by default

        [SerializeField]
        private int rarity = 1;

        [SerializeField]
        private IdGenerator optionIdGenerator = new IdGenerator(1);

        [SerializeField]
        private List<ClassData> requirements = new List<ClassData>();

        [SerializeField]
        private List<OptionData> options = new List<OptionData>();

        [PropertyGroup("Id")]
        [ReadOnlyField]
        public string NameId {
            get {
                return this.nameId.Trim();
            }
            set {
                this.nameId = value?.Trim();
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

        [PropertyGroup("Properties")]
        public string DescriptionId {
            get {
                return this.descriptionId;
            }
            set {
                this.descriptionId = value;
            }
        }

        [PropertyGroup("Properties")]
        public string IconId {
            get {
                return this.iconId;
            }
            set {
                this.iconId = value;
            }
        }

        [PropertyGroup("Properties")]
        public bool Recurring {
            get {
                return this.recurring;
            }
            set {
                this.recurring = value;
            }
        }
        
        [PropertyGroup("Properties")]
        public bool Enabled {
            get {
                return this.enabled;
            }
            set {
                this.enabled = value;
            }
        }

        public List<ClassData> Requirements {
            get {
                return this.requirements;
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

        public List<OptionData> Options {
            get {
                return this.options;
            }
        }

        [PropertyGroup("Properties")]
        [PropertyRenderer("GameEvent.RarityRenderer")]
        public int Rarity {
            get {
                return this.rarity;
            }
            set {
                this.rarity = value;
            }
        }

        public int GenerateNewOptionId() {
            return this.optionIdGenerator.Generate();
        }

        public EventData Duplicate() {
            EventData copy = new EventData();
            TypeUtils.CopyProperties(this, copy);

            // Copy the generator
            copy.optionIdGenerator = this.optionIdGenerator.Duplicate();
            
            // Copy requirements
            copy.requirements = CopyRequirements();
            
            // Copy options
            copy.options = CopyOptions();
            
            return copy;
        }

        private List<ClassData> CopyRequirements() {
            List<ClassData> listCopy = new List<ClassData>(this.requirements.Count);
            ClassData.Copy(this.requirements, listCopy);
            return listCopy;
        }

        private List<OptionData> CopyOptions() {
            List<OptionData> optionsListCopy = new List<OptionData>(this.options.Count);
            for (int i = 0; i < this.options.Count; ++i) {
                OptionData instanceCopy = this.options[i].Duplicate();
                optionsListCopy.Add(instanceCopy);
            }

            return optionsListCopy;
        }
    }
}
