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
        
        [BurstCompile]
        public static ValueTypeOption<DynamicBuffer<T>> GetAsOption<T>(this ref BufferFromEntity<T> self, in Entity entity)
            where T : struct, IBufferElementData {
            if (!self.HasComponent(entity)) {
                return ValueTypeOption<DynamicBuffer<T>>.None;
            }

            DynamicBuffer<T> result = self[entity];
            return ValueTypeOption<DynamicBuffer<T>>.Some(result);
        }
    }
}