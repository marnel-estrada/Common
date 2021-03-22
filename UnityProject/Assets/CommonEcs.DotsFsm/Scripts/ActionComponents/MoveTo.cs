using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs.DotsFsm {
    public struct MoveTo : IComponentData {
        public readonly FixedString64 finishEvent;
        
        public float3 start;
        public float3 destination;
        
        // We denormalized here instead of updating the targetEntity's translation 
        // component right away. This is so that we can run the action in parallel.
        public float3 currentPosition;
        
        public readonly Entity targetEntity; // The entity to move

        public float duration;

        public MoveTo(Entity targetEntity) : this() {
            this.targetEntity = targetEntity;
        }
        
        public MoveTo(Entity targetEntity, FixedString64 finishEvent) : this() {
            this.targetEntity = targetEntity;
            this.finishEvent = finishEvent;
        }

        public void Init(float3 start, float3 destination, float duration) {
            this.start = start;
            this.destination = destination;
            this.duration = duration;
        }
    }
}