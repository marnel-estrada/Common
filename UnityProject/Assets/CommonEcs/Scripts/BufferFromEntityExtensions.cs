using Unity.Burst;
using Unity.Entities;

namespace CommonEcs {
    public static class BufferFromEntityExtensions {
        [BurstCompile]
        public static bool TryGet<T>(this ref BufferFromEntity<T> self, in Entity entity, out DynamicBuffer<T> result)
            where T : struct, IBufferElementData {
            if (!self.HasComponent(entity)) {
                result = default;
                return false;
            }

            result = self[entity];
            return true;
        }
    }
}