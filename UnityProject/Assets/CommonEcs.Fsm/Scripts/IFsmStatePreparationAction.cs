using Unity.Entities;

namespace Common.Ecs.Fsm {
    public interface IFsmStatePreparationAction {
        /// <summary>
        /// Prepares the specified state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="commandBuffer"></param>
        void Prepare(ref FsmState state, ref EntityCommandBuffer.Concurrent commandBuffer, int jobIndex);
        
    }
}
