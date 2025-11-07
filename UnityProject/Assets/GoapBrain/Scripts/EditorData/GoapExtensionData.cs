using System;
using UnityEngine;

namespace GoapBrain {
    [Serializable]
    public class GoapExtensionData {
        [SerializeField]
        private GoapDomainData domainData;

        public GoapDomainData? DomainData {
            get => this.domainData;
            set => this.domainData = value;
        }
    }
}
