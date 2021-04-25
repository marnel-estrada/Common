using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs.DotsFsm {
    public class MoveToSystem : DotsFsmActionSystem<MoveTo, MoveToSystem.Execution> {
        private DotsFsmSystemGroup dotsFsmSystemGroup;

        protected override void OnCreate() {
            base.OnCreate();
            this.dotsFsmSystemGroup = this.World.GetOrCreateSystem<DotsFsmSystemGroup>();
        }

        public struct Execution : IFsmActionExecution<MoveTo> {
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> allTranslation;
            
            [NativeDisableParallelForRestriction]
            public NativeReference<bool> rerunGroup;
            
            public float deltaTime;
            
            public void OnEnter(Entity actionEntity, ref DotsFsmAction action, ref MoveTo move) {
                // Set to start position
                this.allTranslation[move.targetEntity] = new Translation() {
                    Value = move.start
                };
                
                // Note here that the timer was already reset when MoveTo.Init() was invoked 
                if (Comparison.IsZero(move.timer.duration)) {
                    // Duration is zero
                    // Let's finish right away
                    Finish(ref action, ref move);
                }
            }

            public void OnUpdate(Entity actionEntity, ref DotsFsmAction action, ref MoveTo move) {
                move.timer.Update(this.deltaTime);
                
                if (move.timer.HasElapsed) {
                    // Duration is done. Snap to destination.
                    Finish(ref action, ref move);
                    return;
                }
                
                // Timer is not done yet
                // Let's interpolate
                float3 newPosition = math.lerp(move.start, move.destination, move.timer.Ratio);
                this.allTranslation[move.targetEntity] = new Translation() {
                    Value = newPosition
                };
            }

            public void OnExit(Entity actionEntity, DotsFsmAction action, ref MoveTo move) {
            }
            
            private void Finish(ref DotsFsmAction action, ref MoveTo moveTo) {
                // Snap to destination
                this.allTranslation[moveTo.targetEntity] = new Translation() {
                    Value = moveTo.destination
                };
                
                // Send finish event if it exists
                if (moveTo.finishEvent.IsSome) {
                    action.SendEvent(moveTo.finishEvent.ValueOr(default)); 
                    this.rerunGroup.Value = true;
                }
            }
        }

        protected override Execution PrepareActionExecution() {
            // Use the actual deltaTime only when it's the first run of the DotsFsmSystemGroup
            // Note that we allow reruns on some frames so that some cases like movement will look
            // smooth. However, if we use the actual deltaTime on every rerun, some entities will
            // move more than once in every frame. This will look like the agent is moving much
            // faster that it is supposed to.
            float deltaTime = this.dotsFsmSystemGroup.RerunCounter == 0 ? UnityEngine.Time.deltaTime : 0;
            
            return new Execution() {
                allTranslation = GetComponentDataFromEntity<Translation>(),
                rerunGroup = this.dotsFsmSystemGroup.RerunGroup,
                deltaTime = deltaTime
            };
        }
    }
}