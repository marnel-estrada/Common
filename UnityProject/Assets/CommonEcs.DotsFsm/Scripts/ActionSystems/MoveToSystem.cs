using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs.DotsFsm {
    public class MoveToSystem : DotsFsmActionSystem<MoveTo, MoveToSystem.Execution> {
        protected override Execution PrepareActionExecution() {
            return new Execution() {
                allTranslations = GetComponentDataFromEntity<Translation>(),
                allTimers = GetComponentDataFromEntity<DurationTimer>(),
            };
        }
        
        public struct Execution : IFsmActionExecution<MoveTo> {
            public ComponentDataFromEntity<Translation> allTranslations;
            public ComponentDataFromEntity<DurationTimer> allTimers;
            
            public void OnEnter(Entity actionEntity, ref DotsFsmAction action, ref MoveTo moveTo) {
                // Set to start position
                this.allTranslations[moveTo.targetEntity] = new Translation() {
                    Value = moveTo.start
                };

                if (Comparison.IsZero(moveTo.duration)) {
                    // Duration is zero
                    // Let's finish right away
                    Finish(ref action, ref moveTo);
                    return;
                }
                
                // Initialize timer
                DurationTimer timer = this.allTimers[actionEntity];
                timer.Reset(moveTo.duration);
                this.allTimers[actionEntity] = timer; // Modify
            }

            public void OnUpdate(Entity actionEntity, ref DotsFsmAction action, ref MoveTo moveTo) {
                DurationTimer timer = this.allTimers[actionEntity];
                if (timer.HasElapsed) {
                    // Duration is done. Snap to destination.
                    Finish(ref action, ref moveTo);
                    return;
                }
                
                // Timer is not done yet
                // Let's interpolate
                float3 newPosition = math.lerp(moveTo.start, moveTo.destination, timer.Ratio);
                this.allTranslations[moveTo.targetEntity] = new Translation() {
                    Value = newPosition
                };
            }

            private void Finish(ref DotsFsmAction action, ref MoveTo moveTo) {
                // Snap to destination
                this.allTranslations[moveTo.targetEntity] = new Translation() {
                    Value = moveTo.destination
                };
                
                // Send event if it exists
                if (moveTo.finishEvent.Length > 0) {
                    action.SendEvent(moveTo.finishEvent);
                }
            }

            public void OnExit(Entity actionEntity, DotsFsmAction action, ref MoveTo moveTo) {
            }
        }
    }
}