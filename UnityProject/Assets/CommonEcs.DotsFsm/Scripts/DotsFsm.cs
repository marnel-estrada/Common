using CommonEcs;

using Unity.Entities;

namespace Common.Ecs.DotsFsm {
    public struct DotsFsm : IComponentData {
        public ValueTypeOption<Entity> currentState;
    }
}