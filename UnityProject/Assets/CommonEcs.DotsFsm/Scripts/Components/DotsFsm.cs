using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.DotsFsm {
    /// <summary>
    /// An FSM is an entity with this component and a dynamic buffer of DotsFsmTransition
    /// elements.
    /// </summary>
    public struct DotsFsm : IComponentData {
        // We don't use FixedString here to conserve archetype space
        public ValueTypeOption<FsmEvent> pendingEvent;
        public ValueTypeOption<Entity> currentState;

        // To start a state, we set the entity of the state that we want to start
        // This will be consumed by StartFsmSystem
        public ValueTypeOption<Entity> startState;

        /// <summary>
        /// Constructor with specified starting state
        /// </summary>
        /// <param name="stateEntity"></param>
        public DotsFsm(Entity stateEntity) : this() {
            Start(stateEntity);
        }

        public void Start(Entity stateEntity) {
            this.startState = ValueTypeOption<Entity>.Some(stateEntity);
            
            // We set the currentState to None when we start an FSM to remove the retained value
            // in currentState. This is because setting the currentState is done in systems.
            // There may be a case that a code will check the currentState after starting the FSM
            // and they find out that it still retained the value from the previous runs. This
            // results to bugs.
            this.currentState = ValueTypeOption<Entity>.None;
        }

        public void SendEvent(in FsmEvent fsmEvent) {
            if (this.pendingEvent.IsSome) {
                // This means that there could be two actions that sent events
                throw new Exception("Can't replace existing pending event");
            }
            
            this.pendingEvent = ValueTypeOption<FsmEvent>.Some(fsmEvent);
        }

        public void ClearPendingEvent() {
            this.pendingEvent = ValueTypeOption<FsmEvent>.None;
        }

        public void SendEvent(in FixedString64 eventAsString) {
            if (eventAsString.Length == 0) {
                throw new Exception("Can't send empty events");
            }
            
            SendEvent(new FsmEvent(eventAsString));
        }

        public void Stop() {
            // To stop, we just set the currentState to None. By doing this, IdentifyRunningActionsSystem
            // won't be able to tell which action entities should run.
            this.currentState = ValueTypeOption<Entity>.None;
        }
    }
}