using System;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Common.Ecs.Fsm {
    [UpdateAfter(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmActionEndSystem))]
    [UpdateBefore(typeof(MoveActionSystemBarrier))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class MoveActionSystem : FsmActionJobSystem<MoveActionSystem.ActionComposer, MoveActionSystem.MoveJobAction> {
        private MoveActionSystemBarrier barrier;
        
        private ComponentDataFromEntity<Translation> allTranslations;

        protected override void OnCreate() {
            base.OnCreate();
            this.barrier = this.World.GetOrCreateSystem<MoveActionSystemBarrier>();
        }

        protected override EntityQuery GetQuery() {
            return GetEntityQuery(typeof(FsmAction), typeof(MoveAction), typeof(DurationTimer));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.allTranslations = GetComponentDataFromEntity<Translation>();
            
            return base.OnUpdate(inputDeps);
        }
        
        protected override ActionComposer GetJobActionComposer() {
            return new ActionComposer() {
                allTranslations = this.allTranslations,
                moveActionType = GetComponentTypeHandle<MoveAction>(),
                timerType = GetComponentTypeHandle<DurationTimer>()
            };
        }

        public struct ActionComposer : IFsmJobActionComposer<MoveJobAction> {
            public ComponentDataFromEntity<Translation> allTranslations;
            
            public ComponentTypeHandle<MoveAction> moveActionType;
            public ComponentTypeHandle<DurationTimer> timerType;
            
            public MoveJobAction Compose(ArchetypeChunk chunk) {
                return new MoveJobAction() {
                    allTranslations = this.allTranslations,
                    moveActions = chunk.GetNativeArray(this.moveActionType),
                    timers = chunk.GetNativeArray(this.timerType)
                };;
            }
        }

        public struct MoveJobAction : IFsmJobAction {
            public ComponentDataFromEntity<Translation> allTranslations;

            public NativeArray<MoveAction> moveActions;
            public NativeArray<DurationTimer> timers;

            public void DoEnter(int index, ref FsmAction action, ref FsmActionUtility utility) {
                MoveAction moveAction = this.moveActions[index];
                DurationTimer timer = this.timers[index];

                if (Comparison.IsZero(moveAction.duration)) {
                    Finish(index, ref action);
                    return;
                }

                if (VectorUtils.Equals(moveAction.from, moveAction.to)) {
                    // from position and to position are already the same
                    Finish(index, ref action);
                    return;
                }

                if (moveAction.target == Entity.Null) {
                    // There's no target. Finish the action.
                    Finish(index, ref action);
                    return;
                }

                SetPosition(index, moveAction.from);

                // Reset the timer
                timer.Reset(moveAction.duration);
                this.timers[index] = timer; // Modify
            }

            public void DoUpdate(int index, ref FsmAction action, ref FsmActionUtility utility) {
                MoveAction move = this.moveActions[index];
                DurationTimer timer = this.timers[index];
                if (timer.HasElapsed) {
                    // Duration has already lapsed. Snap to destination.
                    Finish(index, ref action);
                    return;
                }

                SetPosition(index, Vector3.Lerp(move.from, move.to, timer.Ratio));
            }

            private void SetPosition(int index, float3 newPosition) {
                MoveAction action = this.moveActions[index];
                Translation translation = this.allTranslations[action.target];

                translation.Value = newPosition;
                this.allTranslations[action.target] = translation; // Set
            }

            private void Finish(int index, ref FsmAction action) {
                MoveAction move = this.moveActions[index];
                SetPosition(index, move.to); // Snap to destination
                this.moveActions[index] = move; // Set the value

                action.finished = true;

                // Sending of event on finish will be handled by MoveActionCheckFinishedSystem
                // It was done this way so we don't duplicate SendEvent() method
            }
        }

        private class MoveActionSystemBarrier : EntityCommandBufferSystem {    
        }
    }
}