using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class GoapExtensionData {

        [SerializeField]
        private List<Condition> preconditions = new List<Condition>();

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

        public List<Condition> Preconditions {
            get {
                return preconditions;
            }
        }

    }
}
