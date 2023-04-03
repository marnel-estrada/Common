using System;
using CommonEcs;
using Unity.Entities;
using Unity.Jobs;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// An abstract base class for systems that affects FSM like FSM action systems but a JobComponentSystem
    /// </summary>
    [UpdateAfter(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmActionEndSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class FsmJobSystem : JobSystemBase {
        protected ComponentDataFromEntity<Fsm> allFsms;
        protected ComponentDataFromEntity<FsmState> allStates;

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.allFsms = GetComponentDataFromEntity<Fsm>();
            this.allStates = GetComponentDataFromEntity<FsmState>();

            return inputDeps;
        }

        /// <summary>
        /// Returns whether or not the action can execute based on the states of its owner FSM State and FSM.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected bool CanExecute(ref FsmAction action) {
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
        protected void SendEvent(ref FsmAction action, uint anEvent) {
            FsmState state = this.allStates[action.stateOwner];
            Fsm fsm = this.allFsms[state.fsmOwner];
            fsm.SendEvent(anEvent);
            this.allFsms[state.fsmOwner] = fsm; // Update the value
        }

        /// <summary>
        /// Resolves the owner of the FSM using the specified action
        /// </summary>
        /// <param name="fsmAction"></param>
        /// <returns></returns>
        protected Entity GetFsmOwner(ref FsmAction fsmAction) {
            FsmState state = this.allStates[fsmAction.stateOwner];
            Fsm fsm = this.allFsms[state.fsmOwner];
            return fsm.owner;
        }
    }
}
