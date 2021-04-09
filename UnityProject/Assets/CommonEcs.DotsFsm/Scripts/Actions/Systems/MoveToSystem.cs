using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(StartFsmSystem))]
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
    [UpdateAfter(typeof(IdentifyRunningActionsSystem))]
    public class MoveToSystem : JobSystemBase {
        private EntityQuery query;
        private DotsFsmSystemGroup dotsFsmSystemGroup;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsmAction), typeof(MoveTo), typeof(DurationTimer));
            this.dotsFsmSystemGroup = this.World.GetOrCreateSystem<DotsFsmSystemGroup>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            // Do the action first
            ComponentTypeHandle<DotsFsmAction> actionHandle = GetComponentTypeHandle<DotsFsmAction>();
            ComponentTypeHandle<MoveTo> moveToHandle = GetComponentTypeHandle<MoveTo>();
            
            ActionJob actionJob = new ActionJob {
                fsmActionHandle = actionHandle,
                moveToHandle = moveToHandle,
                timerHandle = GetComponentTypeHandle<DurationTimer>(),
                rerunGroup = this.dotsFsmSystemGroup.RerunGroup
            };

            JobHandle handle = actionJob.ScheduleParallel(this.query, 1, inputDeps);

            // Apply the position stored in MoveTo.currentPosition (denormalized)
            ApplyToTranslations applyJob = new ApplyToTranslations {
                allTranslations = GetComponentDataFromEntity<Translation>(),
                actionHandle = actionHandle,
                moveToHandle = moveToHandle
            };

            return applyJob.ScheduleParallel(this.query, 1, handle);
        }
        
        [BurstCompile]
        private struct ActionJob : IJobEntityBatch {
            public ComponentTypeHandle<DotsFsmAction> fsmActionHandle;
            public ComponentTypeHandle<MoveTo> moveToHandle;
            public ComponentTypeHandle<DurationTimer> timerHandle;

            [NativeDisableParallelForRestriction]
            public NativeReference<bool> rerunGroup;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<DotsFsmAction> fsmActions = batchInChunk.GetNativeArray(this.fsmActionHandle);
                NativeArray<MoveTo> moveTos = batchInChunk.GetNativeArray(this.moveToHandle);
                NativeArray<DurationTimer> timers = batchInChunk.GetNativeArray(this.timerHandle);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    DotsFsmAction fsmAction = fsmActions[i];
                    MoveTo moveTo = moveTos[i];
                    DurationTimer timer = timers[i];
                    
                    Process(ref fsmAction, ref moveTo, ref timer);
                    
                    // Modify
                    fsmActions[i] = fsmAction;
                    moveTos[i] = moveTo;
                    timers[i] = timer;
                }
            }
            
            private void Process(ref DotsFsmAction fsmAction, ref MoveTo moveTo, ref DurationTimer timer) {
                if (fsmAction.running) {
                    if (!fsmAction.entered) {
                        OnEnter(ref fsmAction, ref moveTo, ref timer);
                        fsmAction.entered = true;
                        fsmAction.exited = false;
                    }

                    OnUpdate(ref fsmAction, ref moveTo, ref timer);
                } else {
                    if (fsmAction.entered && !fsmAction.exited) {
                        // This means the action's state is no longer the FSM's current state
                        // However, the state entered and hasn't exited yet
                        // We're supposed to do OnExit() but the action doesn't need one 
                        fsmAction.exited = true;
                    }
                    
                    // Reset the states
                    fsmAction.entered = false;
                }
            }

            private void OnEnter(ref DotsFsmAction action, ref MoveTo moveTo,
                ref DurationTimer timer) {
                // Set to start position
                moveTo.currentPosition = moveTo.start;

                if (Comparison.IsZero(moveTo.duration)) {
                    // Duration is zero
                    // Let's finish right away
                    Finish(ref action, ref moveTo);
                    return;
                }
                
                // Initialize timer
                timer.Reset(moveTo.duration);
            }
            
            private void OnUpdate(ref DotsFsmAction action, ref MoveTo moveTo,
                ref DurationTimer timer) {
                if (timer.HasElapsed) {
                    // Duration is done. Snap to destination.
                    Finish(ref action, ref moveTo);
                    return;
                }
                
                // Timer is not done yet
                // Let's interpolate
                float3 newPosition = math.lerp(moveTo.start, moveTo.destination, timer.Ratio);
                moveTo.currentPosition = newPosition;
            }
            
            private void Finish(ref DotsFsmAction action, ref MoveTo moveTo) {
                // Snap to destination
                moveTo.currentPosition = moveTo.destination;
                
                // Send event if it exists
                if (moveTo.finishEvent.Length > 0) {
                    action.SendEvent(moveTo.finishEvent);
                    
                    
                    this.rerunGroup.Value = true;
                }
            }
        }
        
        [BurstCompile]
        private struct ApplyToTranslations : IJobEntityBatch {
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> allTranslations;

            [ReadOnly]
            public ComponentTypeHandle<DotsFsmAction> actionHandle;
            
            public ComponentTypeHandle<MoveTo> moveToHandle;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<DotsFsmAction> actions = batchInChunk.GetNativeArray(this.actionHandle);
                NativeArray<MoveTo> moveTos = batchInChunk.GetNativeArray(this.moveToHandle);
                for (int i = 0; i < batchInChunk.Count; ++i) {
                    DotsFsmAction action = actions[i];
                    if (!action.running) {
                        // Apply to only running actions
                        continue;
                    }
                    
                    MoveTo moveTo = moveTos[i];
                    
                    // Apply to translation of target entity
                    this.allTranslations[moveTo.targetEntity] = new Translation {
                        Value = moveTo.currentPosition
                    };
                }
            }
        }
    }
}