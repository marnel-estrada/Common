using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.DotsFsm {
    [UpdateAfter(typeof(StartFsmSystem))]
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
    [UpdateAfter(typeof(IdentifyRunningActionsSystem))]
    public abstract class DotsFsmActionSystem<ActionType, ActionExecutionType> : JobSystemBase
        where ActionType : struct, IComponentData 
        where ActionExecutionType : struct, IFsmActionExecution<ActionType> {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsmAction), typeof(ActionType));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ExecuteActionJob job = new ExecuteActionJob() {
                entityHandle = GetEntityTypeHandle(),
                fsmActionHandle = GetComponentTypeHandle<DotsFsmAction>(),
                customActionHandle = GetComponentTypeHandle<ActionType>(),
                execution = PrepareActionExecution()
            };
            
            return job.Schedule(this.query, inputDeps);
        }

        protected abstract ActionExecutionType PrepareActionExecution(); 

        [BurstCompile]
        public struct ExecuteActionJob : IJobEntityBatchWithIndex {
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            
            public ComponentTypeHandle<DotsFsmAction> fsmActionHandle;
            public ComponentTypeHandle<ActionType> customActionHandle;

            public ActionExecutionType execution;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<Entity> entities = batchInChunk.GetNativeArray(this.entityHandle);
                NativeArray<DotsFsmAction> fsmActions = batchInChunk.GetNativeArray(this.fsmActionHandle);
                NativeArray<ActionType> customActions = batchInChunk.GetNativeArray(this.customActionHandle);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    Entity entity = entities[i];
                    DotsFsmAction fsmAction = fsmActions[i];
                    ActionType customAction = customActions[i];

                    Process(ref fsmAction, entity, ref customAction);
                    
                    // Modify
                    fsmActions[i] = fsmAction;
                    customActions[i] = customAction;
                }
            }

            private void Process(ref DotsFsmAction fsmAction, Entity entity, ref ActionType customAction) {
                if (fsmAction.running) {
                    if (!fsmAction.entered) {
                        this.execution.OnEnter(entity, ref fsmAction, ref customAction);
                        fsmAction.entered = true;
                        fsmAction.exited = false;
                    }

                    // Run OnUpdate()
                    this.execution.OnUpdate(entity, ref fsmAction, ref customAction);
                } else {
                    if (fsmAction.entered && !fsmAction.exited) {
                        // This means the action's state is no longer the FSM's current state
                        // However, the state entered and hasn't exited yet
                        // We do OnExit()
                        this.execution.OnExit(entity, fsmAction, ref customAction);
                        fsmAction.exited = true;
                    }
                    
                    // Reset the states
                    fsmAction.entered = false;
                }
            }
        }
    }
}