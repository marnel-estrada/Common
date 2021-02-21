using Common;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace CommonEcs.DotsFsm {
    [UpdateAfter(typeof(SendEventFromActionsToFsmSystem))]
    public class ConsumePendingEventSystem : SystemBase {
        protected override void OnUpdate() {
            this.Entities.ForEach(delegate(ref DotsFsm fsm, in Name name, in DynamicBuffer<Transition> transitions) {
                if (fsm.pendingEvent.IsNone) {
                    // No event to consume
                    return;
                }

                // The current state will be updated in the matcher
                fsm = fsm.currentState.Match<TryChangeState, DotsFsm>(new TryChangeState() {
                    fsm = fsm,
                    fsmName = name,
                    eventId = fsm.pendingEvent.ValueOr(default),
                    transitions = transitions,
                    allNames = GetComponentDataFromEntity<Name>(true)
                });
            }).ScheduleParallel();
        }

        private struct TryChangeState : IFuncOptionMatcher<Entity, DotsFsm> {
            public DotsFsm fsm;
            public Name fsmName;
            public FixedString64 eventId;
            
            [ReadOnly]
            public DynamicBuffer<Transition> transitions;

            [ReadOnly]
            public ComponentDataFromEntity<Name> allNames;

            public DotsFsm OnSome(Entity currentStateEntity) {
                if (this.eventId.Length == 0) {
                    Debug.LogError("Can't have empty eventId");
                    return this.fsm;
                }
                
                // Look for the entity with the same from state and eventId
                for (int i = 0; i < this.transitions.Length; ++i) {
                    Transition transition = this.transitions[i];
                    if (!(transition.fromState == currentStateEntity && transition.eventId.Equals(this.eventId))) {
                        continue;
                    }

                    // We found a transition
                    this.fsm.currentState = ValueTypeOption<Entity>.Some(transition.toState);
                        
                    // Don't forget to clear the pending event so that actions will run
                    this.fsm.pendingEvent = ValueTypeOption<FixedString64>.None;
                        
                    return this.fsm;
                }
                
                // At this point, there are no transitions found
                // We log a warning
                Name currentStateName = this.allNames[currentStateEntity];
                
                // Burst doesn't like any other string format methods
                // ReSharper disable once UseStringInterpolation
                Debug.LogWarning(string.Format("{0}.{1} does not have transition for event {2}", this.fsmName.value, currentStateName.value, this.eventId));
                
                return this.fsm;
            }

            public DotsFsm OnNone() {
                return this.fsm;
            }
        }
    }
}