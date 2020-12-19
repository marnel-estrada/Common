using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// An interface that prepares an entity for condition resolution
    /// </summary>
    public interface IConditionResolverComposer {
        /// <summary>
        /// Prepares the specified entity
        /// 
        /// Mostly, it's by adding components that resolver systems will then execute
        /// </summary>
        /// <param name="resolverEntity"></param>
        /// <param name="commandBuffer"></param>
        void Prepare(Entity resolverEntity, EntityCommandBuffer commandBuffer);
    }
}