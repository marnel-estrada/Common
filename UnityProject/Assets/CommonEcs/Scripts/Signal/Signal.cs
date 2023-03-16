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
        public static void Dispatch<T>(EntityManager entityManager, T signalComponent) where T : unmanaged, IComponentData {
            Entity entity = entityManager.CreateEntity(typeof(Signal), typeof(T));
            entityManager.SetComponentData(entity, signalComponent);
        }

        /// <summary>
        /// A common way of dispatching a signal using an EntityCommandBuffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="signalComponent"></param>
        /// <typeparam name="T"></typeparam>
        public static void Dispatch<T>(EntityCommandBuffer buffer, T signalComponent) where T : unmanaged, IComponentData {
            Entity entity = buffer.CreateEntity();
            buffer.AddComponent(entity, new Signal());
            buffer.AddComponent(entity, signalComponent);
        }
        
        public static void Dispatch<T>(int jobIndex, EntityCommandBuffer.ParallelWriter buffer, T signalComponent) 
            where T : unmanaged, IComponentData {
            Entity entity = buffer.CreateEntity(jobIndex);
            buffer.AddComponent(jobIndex, entity, new Signal());
            buffer.AddComponent(jobIndex, entity, signalComponent);
        }
    }
}