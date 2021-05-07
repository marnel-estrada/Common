using System.Collections.Generic;

using Common;

using CommonEcs.UtilityBrain;

using Unity.Entities;

using UnityEngine;

namespace GoalSelector {
    /// <summary>
    /// Handles the parsing and management of GoalSelectorData
    /// </summary>
    public class GoalSelectorsCenter : MonoBehaviour {
        [SerializeField]
        private GoalSelectorData[] dataList;

        // These are already the parsed data
        private GoalSelector[] selectors;

        private void Awake() {
            this.selectors = new GoalSelector[this.dataList.Length];
            
            Parse();
        }

        private void Parse() {
            for (int i = 0; i < this.dataList.Length; ++i) {
                this.selectors[i] = Parse(this.dataList[i]);
            }
        }

        private static GoalSelector Parse(GoalSelectorData data) {
            GoalSelector goalSelector = new GoalSelector();
            
            // Parse each goal
            for (int i = 0; i < data.Count; ++i) {
                GoalData goalData = data.GetAt(i);
                Goal goal = new Goal(goalData.Id, goalData.ConditionName, goalData.ConditionValue);
                ParseConsiderations(goalData, goal);
                
                goalSelector.Add(goal);
            }

            return goalSelector;
        }

        private static void ParseConsiderations(GoalData data, Goal goal) {
            IReadOnlyList<ClassData> considerationsData = data.Considerations;
            for (int i = 0; i < considerationsData.Count; i++) {
                Option<ConsiderationAssembler> assemblerInstance = 
                    TypeUtils.Instantiate<ConsiderationAssembler>(considerationsData[i], null);
                assemblerInstance.Match(new AddAssemblerToGoal(goal));
            }
        }
        
        private readonly struct AddAssemblerToGoal : IOptionMatcher<ConsiderationAssembler> {
            private readonly Goal goal;

            public AddAssemblerToGoal(Goal goal) {
                this.goal = goal;
            }

            public void OnSome(ConsiderationAssembler assembler) {
                this.goal.Add(assembler);
            }

            public void OnNone() {
            }
        }

        public Entity CreateUtilityBrainEntity(ref EntityManager entityManager, int dataIndex) {
            return Entity.Null;
        }
    }
}