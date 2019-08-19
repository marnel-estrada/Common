using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GameEvent {
    [Serializable]
    public class EventData {
        private readonly int id;
        
        [SerializeField]
        private string nameId;
        
        [SerializeField]
        private string descriptionId;
        
        [SerializeField]
        private string iconId;
        
        [SerializeField]
        private bool recurring;

        [SerializeField]
        private List<ClassData> requirements;

        public EventData(int id) {
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

        public string IconId {
            get {
                return this.iconId;
            }
            set {
                this.iconId = value;
            }
        }

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
    }
}
