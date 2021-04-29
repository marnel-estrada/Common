using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(StartFsmSystem))]
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
    [UpdateAfter(typeof(IdentifyRunningActionsSystem))]
    public abstract class DotsFsmActionSystem<ActionType, ActionExecutionType> : JobSystemBase
        where ActionType : struct, IFsmActionComponent 
        where ActionExecutionType : struct, IFsmActionExecution<ActionType> {
        private EntityQuery query;
        private DotsFsmSystemGroup systemGroup;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsmAction), typeof(ActionType));
            this.systemGroup = this.World.GetOrCreateSystem<DotsFsmSystemGroup>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ExecuteActionJob job = new ExecuteActionJob() {
                entityHandle = GetEntityTypeHandle(),
                fsmActionHandle = GetComponentTypeHandle<DotsFsmAction>(),
                customActionHandle = GetComponentTypeHandle<ActionType>(),
                execution = PrepareActionExecution()
            };
            
            return this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, 1, inputDeps) 
                : job.Schedule(this.query, inputDeps);
        }

        protected ref NativeReference<bool> RerunGroup {
            get {
                return ref this.systemGroup.RerunGroup;
            }
        }

        protected virtual bool ShouldScheduleParallel {
            get {
                return true;
            }
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
                    Entity actionEntity = entities[i];
                    DotsFsmAction fsmAction = fsmActions[i];
                    ActionType customAction = customActions[i];

                    Process(actionEntity, ref fsmAction, ref customAction);
                    
                    // Modify
                    fsmActions[i] = fsmAction;
                    customActions[i] = customAction;
                }
            }

            private void Process(in Entity actionEntity, ref DotsFsmAction fsmAction, ref ActionType customAction) {
                if (fsmAction.running) {
                    if (!fsmAction.entered) {
                        this.execution.OnEnter(actionEntity, ref fsmAction, ref customAction);
                        fsmAction.entered = true;
                        fsmAction.exited = false;
                    }

                    // Run OnUpdate()
                    this.execution.OnUpdate(actionEntity, ref fsmAction, ref customAction);
                } else {
                    if (fsmAction.entered && !fsmAction.exited) {
                        // This means the action's state is no longer the FSM's current state
                        // However, the state entered and hasn't exited yet
                        // We do OnExit()
                        this.execution.OnExit(actionEntity, fsmAction, ref customAction);
                        fsmAction.exited = true;
                    }
                    
                    // Reset the states
                    fsmAction.entered = false;
                }
            }
        }
    }
}