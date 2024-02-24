using CommonEcs;
using Unity.Entities;
using Unity.Mathematics;

namespace Components {
    /// <summary>
    /// A common component that can be added to entities to move them by speed to a
    /// destination by calling StartMove() and enabling the component.
    /// </summary>
    public struct MoveBySpeed : IComponentData, IEnableableComponent {
        public float3 startPos;
        public float3 destinationPos;

        public Timer timer;

        public void StartMove(float3 startPos, float3 destinationPos, float speed) {
            this.startPos = startPos;
            this.destinationPos = destinationPos;

            DotsAssert.IsTrue(speed > 0); // Prevent divide by zero
            float duration = math.distance(this.destinationPos, this.startPos) / speed;
            this.timer.Reset(duration);
        }

        public bool IsDone => this.timer.HasElapsed;
    }
}