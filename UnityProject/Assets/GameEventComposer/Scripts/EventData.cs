using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GameEvent {
    [Serializable]
    public class EventData : Identifiable, IntIdentifiable {
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

        [SerializeField]
        private int rarity = 1;

        [SerializeField]
        private List<ClassData> requirements;

        [SerializeField]
        private List<OptionData> options;

        [PropertyGroup("Id")]
        [ReadOnlyField]
        public string NameId {
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
    }
}
