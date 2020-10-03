using Unity.Entities;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// Contains common methods for handling FSM action
    /// It was done this way so we can use it inside job structs
    /// </summary>
    public struct FsmActionUtility {
        public ComponentDataFromEntity<Fsm> allFsms;
        public ComponentDataFromEntity<FsmState> allStates;

        public EntityCommandBuffer.ParallelWriter commandBuffer;
        
        /// <summary>
        /// Returns whether or not the action can execute based on the states of its owner FSM State and FSM.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool CanExecute(ref FsmAction action) {
            FsmState state = this.allStates[action.stateOwner];
            Fsm fsm = this.allFsms[state.fsmOwner];

            if (fsm.currentEvent != Fsm.NULL_EVENT) {
                // This means that the FSM has an unconsumed event and must be consumed
                return false;
            }

            // The action can only execute if the FSMs current state is the state that owned the action
            return fsm.currentState == action.stateOwner;
        }

        /// <summary>
        /// Sends an event to the owner of the action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="anEvent"></param>
        public void SendEvent(int jobIndex, ref FsmAction action, uint anEvent) {
            FsmState state = this.allStates[action.stateOwner];
            Fsm fsm = this.allFsms[state.fsmOwner];
            fsm.SendEvent(anEvent);
            this.allFsms[state.fsmOwner] = fsm; // Update the value

            // We add this tag component so it will be filtered in FsmConsumeEventSystem
            this.commandBuffer.AddComponent(jobIndex, state.fsmOwner, new HasFsmEvent());
        }
    }
}