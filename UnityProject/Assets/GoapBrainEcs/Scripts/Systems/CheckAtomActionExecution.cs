using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// Checks the atom actions whether to proceed to next atom action or continue on the next frame.
    /// </summary>
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    public class CheckAtomActionExecution : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<AtomAction> atomActionType;

        private ComponentDataFromEntity<AtomActionSetExecution> allAtomSetExecutions;
        
        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(AtomAction));
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.atomActionType = GetComponentTypeHandle<AtomAction>(true);

            this.allAtomSetExecutions = GetComponentDataFromEntity<AtomActionSetExecution>(true);
        }

        private NativeArray<Entity> entities;
        private NativeArray<AtomAction> atomActions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.atomActions = chunk.GetNativeArray(this.atomActionType);
        }

        protected override void Process(int index) {
            AtomAction action = this.atomActions[index];

            // Note here that if status is running, we just let the atom action continue on the next frame
            switch (action.status) {
                case GoapStatus.SUCCESS:
                    DoOnSuccess(index, ref action);
                    break;
                    
                case GoapStatus.FAILED:
                    DoOnFail(index, ref action);
                    break;
            }
        }

        private void DoOnSuccess(int index, ref AtomAction action) {
            // Tell parent AtomActionSetExecution to move to the next atom action
            this.PostUpdateCommands.AddComponent(action.parentAtomActionSetExecution, new ExecuteNextAtomAction());
            this.PostUpdateCommands.DestroyEntity(this.entities[index]); // Destroy because it's already done
        }
        
        private void DoOnFail(int index, ref AtomAction action) {
            // Fail the whole PlanExecution
            // We just look for the PlanExecution parent entity and add PlanExecutionFailed component to that
            // entity
            AtomActionSetExecution atomSetExecution = this.allAtomSetExecutions[action.parentAtomActionSetExecution];
            
            // Prepare the parent plan execution for on fail actions execution
            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(atomSetExecution.parentPlanExecution);
            
            this.PostUpdateCommands.AddComponent(atomSetExecution.parentPlanExecution, new OnFailActionExecution(actions.Length));
            
            // We added this component so that ExecuteNextOnFailActionSystem will run immediately
            this.PostUpdateCommands.AddComponent(atomSetExecution.parentPlanExecution, new ExecuteNextActionOnFail());
            
            // Destroy the atom action entity because it's already done
            this.PostUpdateCommands.DestroyEntity(this.entities[index]);
        }
    }
}