using Common;
using System;

using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class ConditionResolverData {
        [SerializeField]
        private string conditionName;

        [SerializeField]
        private ClassData resolver;

        public string ConditionName {
            get {
                return this.conditionName;
            }

            set {
                this.conditionName = value;
            }
        }

        public ClassData ResolverClass {
            get {
                return this.resolver;
            }

            set {
                this.resolver = value;
            }
        }
    }
}
