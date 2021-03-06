using System.Collections.Generic;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// This master container of goap domains is made to remove the scene dependency of the goap domains.
    /// </summary>
    [CreateAssetMenu(menuName = "GameManager/GoapDomainDataContainer")]
    public class GoapDomainDataContainer : ScriptableObject {
        [SerializeField]
        private List<GoapDomainData>? dataList;

        public List<GoapDomainData>? DataList => this.dataList;
    }
}