using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A component that identifies that a certain entity is a signal
    /// </summary>
    public struct Signal : IComponentData {
        /// <summary>
        /// A common way of dispatching a signal using an EntityManager.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="signalComponent"></param>
        /// <typeparam name="T"></typeparam>
        public static void Dispatch<T>(EntityManager entityManager, T signalComponent) where T : struct, IComponentData {
            Entity entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new Signal());
            entityManager.AddComponentData(entity, signalComponent);
        }

        /// <summary>
        /// A common way of dispatching a signal using an EntityCommandBuffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="signalComponent"></param>
        /// <typeparam name="T"></typeparam>
        public static void Dispatch<T>(EntityCommandBuffer buffer, T signalComponent) where T : struct, IComponentData {
            buffer.CreateEntity();
            buffer.AddComponent(new Signal());
            buffer.AddComponent(signalComponent);
        }
    }
}