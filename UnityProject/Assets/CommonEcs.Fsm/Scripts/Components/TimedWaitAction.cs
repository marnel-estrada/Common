using CommonEcs;

using Unity.Entities;

namespace Common.Ecs.Fsm {
    public struct TimedWaitAction : IComponentData {

        public float duration;
        public uint finishEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="finishEvent"></param>
        public TimedWaitAction(float duration, uint finishEvent) {
            this.duration = duration;
            this.finishEvent = finishEvent;
        }

        /// <summary>
        /// Utility method for adding the TimedWaitAction
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="duration"></param>
        /// <param name="finishEvent"></param>
        public static void AddAsAction(EntityCommandBuffer postCommandBuffer, float duration, uint finishEvent) {
            DurationTimer timer = new DurationTimer();
            postCommandBuffer.AddComponent(timer);

            TimedWaitAction waitAction = new TimedWaitAction(duration, finishEvent);
            postCommandBuffer.AddComponent(waitAction);
        }

    }
}
