using Unity.Entities;

namespace Common.Ecs.Fsm {
    // This is only used for RegisterGenericJobType
    public struct DummyComponent : IComponentData {
        private byte dummy;
    }

    // This is only used for RegisterGenericJobType
    public struct DummyPreparationAction : IFsmStatePreparationAction {
        private byte dummy;
        
        public void Prepare(ref FsmState state, ref EntityCommandBuffer.ParallelWriter commandBuffer, int jobIndex) {
        }
    }    
}