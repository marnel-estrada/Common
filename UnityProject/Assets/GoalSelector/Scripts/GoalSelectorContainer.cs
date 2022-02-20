using System.Collections.Generic;
using UnityEngine;

namespace GoalSelector {
    /// <summary>
    /// This master container of goal selectors is made to remove the scene dependency of the goal selectors.
    /// </summary>
    [CreateAssetMenu(menuName = "GameManager/GoalSelectorsContainer")]
    public class GoalSelectorContainer : ScriptableObject {
        [SerializeField]
        private List<GoalSelectorData>? dataList;

        public List<GoalSelectorData>? DataList => this.dataList;
    }
}