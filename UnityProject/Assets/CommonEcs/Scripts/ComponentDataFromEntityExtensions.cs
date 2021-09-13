using Unity.Burst;
using Unity.Entities;

namespace CommonEcs {
    public static class ComponentDataFromEntityExtensions {
        /// <summary>
        /// Tries to get component from ComponentDataFromEntity but checks first if the entity exists.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [BurstCompile]
        public static bool TryGet<T>(this in ComponentDataFromEntity<T> self, in Entity entity, out T component)
            where T : struct, IComponentData {
            if (!self.HasComponent(entity)) {
                component = default;
                return false;
            }

            component = self[entity];
            return true;
        }
    }
}