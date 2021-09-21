using System.Collections.Generic;
using GoalSelector;
using UnityEngine;

namespace Game {
    /// <summary>
    /// This master container of goal selectors is made to remove the scene dependency of the goal selectors.
    /// </summary>
    [CreateAssetMenu(menuName = "GoalSelector/GoalSelectorsContainer")]
    public class GoalSelectorContainer : ScriptableObject {
        [SerializeField]
        private List<GoalSelectorData>? dataList;

        [SerializeField]
        private string? goalSelectorsPath = "Assets/Game/ScriptableObjects/GoalSelector/";

        public List<GoalSelectorData>? DataList => this.dataList;

        public string? GoalSelectorsPath => this.goalSelectorsPath;
    }
}