using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// We need this system because sending event is denormalized in DotsFsmAction
    /// The event is stored there instead and will be consumed here so that we don't need a
    /// reference to DotsFsm whenever we execute actions.
    /// </summary>
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(StartFsmSystem))]
    public partial class SendEventFromActionsToFsmSystem : SystemBase {
        private EntityQuery actionsQuery;
        
        protected override void OnCreate() {
            base.OnCreate();

            this.actionsQuery = GetEntityQuery(typeof(DotsFsmAction));
        }

        protected override void OnUpdate() {
            Job job = new Job() {
                actionType = GetComponentTypeHandle<DotsFsmAction>(),
                allFsms = GetComponentDataFromEntity<DotsFsm>()
            };
            this.Dependency = job.ScheduleParallel(this.actionsQuery, this.Dependency);
        } 

        [BurstCompile]
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<DotsFsmAction> actionType;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<DotsFsm> allFsms;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<DotsFsmAction> actions = batchInChunk.GetNativeArray(this.actionType);
                for (int i = 0; i < actions.Length; ++i) {
                    DotsFsmAction action = actions[i];
                    if (action.pendingEvent.IsSome) {
                        DotsFsm fsm = this.allFsms[action.fsmEntity];
                        if (fsm.pendingEvent.IsSome) {
                            // Can't replace existing event
                            // This means that there may more than one action that sent an
                            // event
                            continue;
                        }
                        
                        fsm.SendEvent(action.pendingEvent.ValueOrError());
                        this.allFsms[action.fsmEntity] = fsm; // Modify
                    }
                    
                    // Clear the pending event
                    action.pendingEvent = ValueTypeOption<FsmEvent>.None;
                    actions[i] = action; // Modify
                }
            }
        }
    }
}