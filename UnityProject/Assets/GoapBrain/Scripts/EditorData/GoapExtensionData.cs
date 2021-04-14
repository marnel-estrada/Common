using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class GoapExtensionData {

        [SerializeField]
        private List<ConditionData> preconditions = new List<ConditionData>();

        [SerializeField]
        private GoapDomainData domainData;

        public GoapDomainData DomainData {
            get {
                return domainData;
            }

            set {
                this.domainData = value;
            }
        }

        public List<ConditionData> Preconditions {
            get {
                return preconditions;
            }
        }

    }
}
