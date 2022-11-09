using Unity.Entities;

using UnityEngine;

namespace CommonEcs.Goap {
    // [GenerateAuthoringComponent]
    public struct GoapAgent : IComponentData {
        // Note here that the first goal is the main goal. Then if it can't resolve
        // actions, it will try to resolve the next goals.
        public ConditionList5 goals;

        public BlobAssetReference<GoapDomainDatabase> domainDbReference;

        // We use a separate entity here because we don't want the agent entity to get
        // bigger and thus will have less entities per archetype.
        // Note that the planner entity contains a BoolHashMap which is a big object.
        public NonNullEntity plannerEntity;

        // Maps to a GoapDomain
        public int domainId; 

        public AgentState state;
        
        // Needed for action execution
        public int currentActionIndex;
        public int currentAtomActionIndex;
        public GoapResult lastResult;

        // This will be consumed by a system
        public bool replanRequested;

        public GoapAgent(in BlobAssetReference<GoapDomainDatabase> domainDbReference, int domainId, in Entity plannerEntity) : this() {
            this.domainDbReference = domainDbReference;
            this.domainId = domainId;
            this.plannerEntity = plannerEntity;
            this.goals = new ConditionList5();
            
            this.state = AgentState.IDLE;
        }

        public void ClearGoals() {
            this.goals.Clear();
        }

        public void SetMainGoal(Condition condition) {
            // We can't just set at zero right away because the list does a bounds check
            // It will cause out of bounds exception if we set element at zero when the list
            // has no items yet
            if (this.goals.Count == 0) {
                // We don't use AddGoal() here because it adds twice to make room for main goal.
                this.goals.Add(condition);
            } else {
                this.goals[0] = condition;
            }
        }

        public void AddGoal(Condition goalCondition) {
            if (this.goals.Count == 0) {
                // We add twice here because the first is space for the main goal
                this.goals.Add(goalCondition);
                this.goals.Add(goalCondition);
            } else {
                this.goals.Add(goalCondition);
            }
        }

        public readonly GoapDomain Domain {
            get {
                return this.domainDbReference.Value.domains[this.domainId];
            }
        }

        public readonly Condition GetGoal(int index) {
            return this.goals[index];
        }

        public void Replan() {
            this.replanRequested = true;
            this.lastResult = GoapResult.FAILED;
            
            // We set to Cleanup so actions have a chance to cleanup before planning again
            this.state = AgentState.CLEANUP;
        }
    }

    public class GoapAgentAuthoring : MonoBehaviour {
        public int domainId;
    }
}