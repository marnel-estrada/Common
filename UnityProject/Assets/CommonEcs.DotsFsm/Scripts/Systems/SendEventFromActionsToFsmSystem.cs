using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// We need this system because sending event is denormalized in DotsFsmAction
    /// The event is stored there instead and will be consumed here so that we don't need a
    /// reference to DotsFsm whenever we execute actions.
    /// </summary>
    [UpdateAfter(typeof(StartFsmSystem))]
    public class SendEventFromActionsToFsmSystem : SystemBase {
        private EntityQuery actionsQuery;
        
        protected override void OnCreate() {
            base.OnCreate();

            this.actionsQuery = GetEntityQuery(typeof(DotsFsmAction));
        }

        protected override void OnUpdate() {
            ValueTypeOption<FixedString64> nonePendingEvent = ValueTypeOption<FixedString64>.None;
            
            // Clear pending events from FSMs first
            // We do this because pending events should only be set once
            this.Entities.ForEach(delegate(ref DotsFsm fsm) {
                fsm.pendingEvent = nonePendingEvent;
            }).ScheduleParallel();

            Job job = new Job() {
                actionType = GetComponentTypeHandle<DotsFsmAction>(),
                allFsms = GetComponentDataFromEntity<DotsFsm>()
            };
            this.Dependency = job.ScheduleParallel(this.actionsQuery, 1, this.Dependency);
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
                        DotsFsm fsm = this.allFsms[action.fsmOwner];
                        if (fsm.pendingEvent.IsSome) {
                            // Can't replace existing event
                            // This means that there may more than one action that sent an
                            // event
                            throw new Exception("Can't replace existing event");
                        }
                        
                        fsm.pendingEvent = action.pendingEvent;
                        this.allFsms[action.fsmOwner] = fsm; // Modify
                    }
                    
                    // Clear the pending event
                    action.pendingEvent = ValueTypeOption<FixedString64>.None;
                    actions[i] = action; // Modify
                }
            }
        }
    }
}