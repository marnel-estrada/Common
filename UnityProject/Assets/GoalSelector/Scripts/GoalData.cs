using System;
using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GoalSelector {
    /// <summary>
    /// Represents an option amongst different options in a goal selector.
    /// This is implemented as a UtilityOption upon parsing.
    /// </summary>
    [Serializable]
    public class GoalData : IDataPoolItem, IDuplicable<GoalData> {
        [SerializeField]
        private string id;

        [SerializeField]
        private int intId;

        [SerializeField]
        private string conditionName;

        [SerializeField]
        private bool conditionValue;

        [SerializeField]
        private string comment;

        [SerializeField]
        private bool showComment;

        [SerializeField]
        private bool editComment;

        [SerializeField]
        private List<ClassData> considerations = new List<ClassData>();

        public string Id {
            get {
                return this.id;
            }
            set {
                this.id = value;
            }
        }

        [PropertyGroup("Effect")]
        public string ConditionName {
            get {
                return this.conditionName;
            }
            set {
                this.conditionName = value;
            }
        }

        [PropertyGroup("Effect")]
        public bool ConditionValue {
            get {
                return this.conditionValue;
            }
            set {
                this.conditionValue = value;
            }
        }

        [Hidden]
        public string Comment {
            get {
                return this.comment;
            }
            set {
                this.comment = value;
            }
        }

        [Hidden]
        public bool ShowComment {
            get {
                return this.showComment;
            }
            set {
                this.showComment = value;
            }
        }

        [Hidden]
        public bool EditComment {
            get {
                return this.editComment;
            }
            set {
                this.editComment = value;
            }
        }

        public List<ClassData> Considerations {
            get {
                return this.considerations;
            }
        }

        [ReadOnlyField]
        [PropertyGroup("ID")]
        public int IntId {
            get {
                return this.intId;
            }

            set {
                this.intId = value;
            }
        }

        public GoalData Duplicate() {
            // Not really implemented for now
            return new GoalData();
        }
    }
}