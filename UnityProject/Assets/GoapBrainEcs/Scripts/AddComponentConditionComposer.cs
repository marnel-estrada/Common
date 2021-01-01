using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// A generic IConditionResolverComposer that merely adds a component with default value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AddComponentConditionComposer<T> : IConditionResolverComposer where T : struct, IComponentData {
        public void Prepare(Entity resolverEntity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent<T>(resolverEntity);
        }
    }
}