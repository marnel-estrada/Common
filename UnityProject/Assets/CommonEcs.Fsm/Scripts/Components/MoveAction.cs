using Unity.Entities;
using Unity.Mathematics;

namespace Common.Ecs.Fsm {
    public struct MoveAction : IComponentData {
        public Entity target; // The object to move
        public float3 from;
        public float3 to;
        public float duration;
        public uint finishEvent;
    }
}
