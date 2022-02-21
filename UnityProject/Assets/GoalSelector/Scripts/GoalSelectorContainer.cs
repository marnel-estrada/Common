using System.Collections.Generic;
using Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GoalSelector {
    /// <summary>
    /// This master container of goal selectors is made to remove the scene dependency of the goal selectors.
    /// </summary>
    [CreateAssetMenu(menuName = "GameManager/GoalSelectorsContainer")]
    public class GoalSelectorContainer : ScriptableObject, IGameManagerScriptableObject {
        [SerializeField]
        private List<GoalSelectorData>? dataList;

        [ShowInInspector]
        public string? Path { get; set; } = "Assets/Game/ScriptableObjects/GoalSelector/";

        public List<GoalSelectorData>? DataList => this.dataList;
    }
}