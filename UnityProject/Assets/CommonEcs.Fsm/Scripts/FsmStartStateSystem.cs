using System;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// System that handles starting of FSM
    /// </summary>
    [UpdateBefore(typeof(FsmConsumeEventSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    class FsmStartStateSystem : TemplateComponentSystem {
        private ComponentDataFromEntity<Fsm> allFsms;

        private ComponentTypeHandle<FsmState> stateType;

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(FsmState), typeof(StartState));
        }

        protected override void BeforeChunkTraversal() {
            this.allFsms = GetComponentDataFromEntity<Fsm>();
            this.stateType = GetComponentTypeHandle<FsmState>();
        }

        private NativeArray<FsmState> states;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.states = chunk.GetNativeArray(this.stateType);
        }

        protected override void Process(int index) {
            FsmState state = this.states[index];
            Fsm fsm = this.allFsms[state.fsmOwner];

            fsm.currentState = state.entityOwner;
            fsm.currentEvent = Fsm.NULL_EVENT;
            this.allFsms[state.fsmOwner] = fsm; // Update the FSM data

            // Add StateJustTransitioned to the state's entity so that the state action prepare system
            // can filter the state
            this.PostUpdateCommands.AddComponent(state.entityOwner, new StateJustTransitioned());

            // Remove this tag component so it will not be started again
            this.PostUpdateCommands.RemoveComponent<StartState>(state.entityOwner);
        }
    }
}
