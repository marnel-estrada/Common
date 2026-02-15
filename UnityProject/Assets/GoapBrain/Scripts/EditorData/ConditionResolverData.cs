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
            get => this.conditionName;

            set => this.conditionName = value;
        }

        public ClassData ResolverClass {
            get => this.resolver;

            set => this.resolver = value;
        }

        public ConditionResolverData CreateCopy() {
            ClassData resolverCopy = new();
            this.resolver.CopyTo(resolverCopy);

            return new ConditionResolverData() {
                conditionName = this.conditionName,
                resolver = resolverCopy
            };
        }
    }
}
