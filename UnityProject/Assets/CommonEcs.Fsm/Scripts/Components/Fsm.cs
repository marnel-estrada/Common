using Unity.Entities;

using UnityEngine.Assertions;

namespace Common.Ecs.Fsm {
    public struct Fsm : IComponentData {

        // The value 0 is reserved as a null event
        public const uint NULL_EVENT = 0;

        public Entity owner; // The entity that owned the FSM

        public Entity currentState;

        // Event is stored until it is consumed by FsmConsumeEventSystem
        public uint currentEvent;

        /// <summary>
        /// Sends an event that causes transition
        /// </summary>
        /// <param name="anEvent"></param>
        public void SendEvent(uint anEvent) {
            Assert.IsTrue(anEvent != NULL_EVENT); // Can't be a null event
            this.currentEvent = anEvent;
        }

    }
}
