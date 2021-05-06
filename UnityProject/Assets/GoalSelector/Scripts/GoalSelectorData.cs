using System.Collections.Generic;

using UnityEngine;

namespace GoalSelector {
    [CreateAssetMenu(menuName = "GoalSelector/GoalSelector")]
    public class GoalSelectorData : ScriptableObject {
        [SerializeField]
        private List<GoalData> goals = new List<GoalData>();
    }
}