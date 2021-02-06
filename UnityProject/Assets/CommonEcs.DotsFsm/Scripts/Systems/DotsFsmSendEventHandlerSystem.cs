using Common;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace CommonEcs.DotsFsm {
    [UpdateBefore(typeof(DestroySignalsSystem))]
    public class DotsFsmSendEventHandlerSystem : SignalHandlerBatchJobSystem<SendEvent, DotsFsmSendEventHandlerSystem.Processor> {
        protected override Processor PrepareProcessor() {
            return new Processor() {
                allFsms = GetComponentDataFromEntity<DotsFsm>(),
                allNames = GetComponentDataFromEntity<Name>(),
                allTransitions = GetBufferFromEntity<Transition>()
            };
        }
        
        public struct Processor : ISignalProcessor<SendEvent> {
            public ComponentDataFromEntity<DotsFsm> allFsms;

            [ReadOnly]
            public ComponentDataFromEntity<Name> allNames;
            
            [ReadOnly]
            public BufferFromEntity<Transition> allTransitions;
            
            public void Execute(Entity signalEntity, SendEvent parameter) {
                DotsFsm fsm = this.allFsms[parameter.fsmEntity];
                DynamicBuffer<Transition> transitions = this.allTransitions[parameter.fsmEntity];
                fsm.currentState.Match(new TryChangeState() {
                    transitions = transitions,
                    parameter = parameter,
                    allFsms = this.allFsms,
                    allNames = this.allNames
                });
            }
        }

        private struct TryChangeState : IOptionMatcher<Entity> {
            [ReadOnly]
            public DynamicBuffer<Transition> transitions;
            
            public SendEvent parameter;
            public ComponentDataFromEntity<DotsFsm> allFsms;
            
            [ReadOnly]
            public ComponentDataFromEntity<Name> allNames;

            public void OnSome(Entity currentStateEntity) {
                // Look for the entity with the same from state and eventId
                for (int i = 0; i < this.transitions.Length; ++i) {
                    Transition transition = this.transitions[i];
                    if (transition.fromState == currentStateEntity && transition.eventId.Equals(this.parameter.eventId)) {
                        // We found a transition
                        DotsFsm fsm = this.allFsms[this.parameter.fsmEntity];
                        fsm.currentState = ValueTypeOption<Entity>.Some(transition.toState);
                        
                        // Don't forget to modify
                        this.allFsms[this.parameter.fsmEntity] = fsm;

                        return;
                    }
                }
                
                // At this point, there are no transitions found
                // We log a warning
                Name fsmName = this.allNames[this.parameter.fsmEntity];
                Name currentStateName = this.allNames[currentStateEntity];
                Debug.LogWarning($"{fsmName.value.ToString()}.{currentStateName.value.ToString()} does not have transition for event {this.parameter.eventId.ToString()}");
            }

            public void OnNone() {
                // This means that the FSM does not have a current entity
                // Can't transition
            }
        }
    }
}