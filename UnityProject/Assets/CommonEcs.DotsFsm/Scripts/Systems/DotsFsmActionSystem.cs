using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.DotsFsm {
    [UpdateAfter(typeof(StartFsmSystem))]
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
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
                allStates = GetComponentDataFromEntity<DotsFsmState>(),
                allFsms = GetComponentDataFromEntity<DotsFsm>(),
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

            [ReadOnly]
            public ComponentDataFromEntity<DotsFsmState> allStates;
            
            [ReadOnly]
            public ComponentDataFromEntity<DotsFsm> allFsms;

            public ActionExecutionType execution;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<Entity> entities = batchInChunk.GetNativeArray(this.entityHandle);
                NativeArray<DotsFsmAction> fsmActions = batchInChunk.GetNativeArray(this.fsmActionHandle);
                NativeArray<ActionType> customActions = batchInChunk.GetNativeArray(this.customActionHandle);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    Entity entity = entities[i];
                    DotsFsmAction fsmAction = fsmActions[i];
                    ActionType customAction = customActions[i];

                    DotsFsmState state = this.allStates[fsmAction.stateOwner];
                    DotsFsm fsm = this.allFsms[state.fsmOwner];

                    Process(fsm, ref fsmAction, entity, ref customAction);
                    
                    // Modify
                    fsmActions[i] = fsmAction;
                    customActions[i] = customAction;
                }
            }

            private void Process(DotsFsm fsm, ref DotsFsmAction fsmAction, Entity entity, ref ActionType customAction) {
                if (fsm.pendingEvent.IsSome) {
                    // This means that the FSM is about to transition but hasn't yet
                    // We don't run the actions until the transition has happened
                    return;
                }
                
                if (fsm.currentState.Match<IsEntityEqualTo, bool>(new IsEntityEqualTo(fsmAction.stateOwner))) {
                    // The state the current state of the FSM. We process.
                    if (!fsmAction.entered) {
                        this.execution.OnEnter(entity, fsmAction, ref customAction);
                        fsmAction.entered = true;
                        fsmAction.exited = false;
                    }

                    // Run OnUpdate()
                    this.execution.OnUpdate(entity, fsmAction, ref customAction);
                } else {
                    if (fsmAction.entered && !fsmAction.exited) {
                        // This means the action's state is no longer the FSM's current state
                        // However, the state entered and hasn't exited yet
                        // We do OnExit()
                        this.execution.OnExit(entity, fsmAction, ref customAction);
                        
                        // Reset the states
                        fsmAction.entered = false;
                        fsmAction.exited = true;
                    }
                }
            }
        }
    }
}