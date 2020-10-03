using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.Fsm {
    [UpdateBefore(typeof(FsmActionStartSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class FsmConsumeEventSystem : TemplateComponentSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<Fsm> fsmType;
        private BufferTypeHandle<FsmTransition> transitionType;

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(Fsm), ComponentType.ReadOnly<HasFsmEvent>(), 
                ComponentType.ReadOnly<FsmTransition>());
        }

        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.fsmType = GetComponentTypeHandle<Fsm>();
            this.transitionType = GetBufferTypeHandle<FsmTransition>();
        }

        private NativeArray<Entity> entities;
        private NativeArray<Fsm> fsms;
        private BufferAccessor<FsmTransition> transitionBuffers;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.fsms = chunk.GetNativeArray(this.fsmType);
            this.transitionBuffers = chunk.GetBufferAccessor(this.transitionType);
        }

        protected override void Process(int index) {
            Fsm fsm = this.fsms[index];

            if (fsm.currentEvent != Fsm.NULL_EVENT) {
                // Loop through all transitions and perform the transition
                DynamicBuffer<FsmTransition> transitions = this.transitionBuffers[index];
                for(int transitionIndex = 0; transitionIndex < transitions.Length; ++transitionIndex) {
                    FsmTransition transition = transitions[transitionIndex];

                    if (fsm.currentState == transition.fromState && fsm.currentEvent == transition.transitionEvent) {
                        // This means that the current state of the fsm corresponds to the "from state" of the transition
                        // and FSM's event corresponds to the transition's event as well
                        // This means that we have to change the state of the FSM

                        fsm.currentState = transition.toState;
                        fsm.currentEvent = Fsm.NULL_EVENT; // Reset
                        this.fsms[index] = fsm; // Update the actual FSM

                        // Add StateJustTransitioned to the state's entity so that the state action prepare system
                        // can filter the state and add the appropriate actions
                        this.PostUpdateCommands.AddComponent(transition.toState, new StateJustTransitioned());
                    }
                }
            }

            // Remove the component so it will no longer be processed in this system
            this.PostUpdateCommands.RemoveComponent<HasFsmEvent>(this.entities[index]);
        }
    }
}
