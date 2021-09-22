using System.Collections.Generic;
using Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// This master container of goap domains is made to remove the scene dependency of the goap domains.
    /// </summary>
    [CreateAssetMenu(menuName = "GameManager/GoapDomainDataContainer")]
    public class GoapDomainDataContainer : ScriptableObject, IGameManagerScriptableObject {
        [SerializeField]
        private List<GoapDomainData>? dataList;

        [ShowInInspector]
        public string? Path { get; set; } = "Assets/Game/ScriptableObjects/Goap/";

        public List<GoapDomainData>? DataList => this.dataList;
    }
}