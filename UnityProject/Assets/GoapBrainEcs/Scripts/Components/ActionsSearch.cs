using Common;

using CommonEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// Will require a DynamicBuffer of ActionEntry which represents the action plan
    /// </summary>
    public struct ActionsSearch : IComponentData {
        // The conditions to search for
        public readonly ConditionList10 targetConditions;
        
        // The parent action search so we know where to continue when this action search is done
        public readonly Entity parentSearch;

        // The entity with the GoapPlanRequest component
        public readonly Entity planRequestEntity;

        // The index of the condition currently being searched
        public short currentConditionIndex;

        public short currentActionIndex;
        
        public ByteBool actionSearchDone;
        public ByteBool success; // Whether action search was successful or not (false means failed)

        public ActionsSearch(ConditionList10 targetConditions, Entity planRequestEntity, Entity parentSearch) {
            this.targetConditions = targetConditions;
            this.planRequestEntity = planRequestEntity;
            this.parentSearch = parentSearch;

            this.currentConditionIndex = -1;

            this.currentActionIndex = -1;
            
            this.actionSearchDone = false;
            this.success = false;
        }

        /// <summary>
        /// Returns whether or not the ActionSearch is the root search (terminal search)
        /// </summary>
        public bool IsRoot {
            get {
                return this.parentSearch == Entity.Null;
            }
        }

        public Condition CurrentTargetCondition {
            get {
                return this.targetConditions[this.currentConditionIndex];
            }
        }

        public void MarkAsFailed() {
            this.actionSearchDone = true;
            this.success = false;
        }
        
        public void MarkAsSuccess() {
            this.actionSearchDone = true;
            this.success = true;
        }

        /// <summary>
        /// Common algorithm for creating an ActionsSearch entity
        /// </summary>
        /// <param name="commandBuffer"></param>
        /// <param name="searchConditions"></param>
        /// <param name="planRequestEntity"></param>
        /// <param name="parentSearchEntity"></param>
        /// <returns></returns>
        public static Entity Create(EntityCommandBuffer commandBuffer, ConditionList10 searchConditions, Entity planRequestEntity, 
            Entity parentSearchEntity) {
            Entity searchEntity = commandBuffer.CreateEntity();
            ActionsSearch actionsSearch = new ActionsSearch(searchConditions, planRequestEntity, parentSearchEntity);
            commandBuffer.AddComponent(searchEntity, actionsSearch);
            
            // List of actions
            commandBuffer.AddBuffer<ActionEntry>(searchEntity);
            
            // HashSet of action effects
            commandBuffer.AddComponent(searchEntity, new ConditionHashSet());
            
            // We add this component to start resolving the first goal condition right away
            commandBuffer.AddComponent(searchEntity, new ResolveNextCondition());

            return searchEntity;
        }
    }
}