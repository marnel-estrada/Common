using Unity.Entities;

namespace Common.Ecs.Fsm {
    public static class IFsmStatePreparationActionExtensions {
        /// <summary>
        /// Adds an action to the specified state entity
        /// </summary>
        /// <param name="prepareAction"></param>
        /// <param name="stateEntity"></param>
        /// <param name="commandBuffer"></param>
        public static Entity AddAction<T>(this T prepareAction, ref Entity stateEntity, ref EntityCommandBuffer.ParallelWriter commandBuffer, int jobIndex)
            where T : struct, IFsmStatePreparationAction {
            Entity entity = commandBuffer.CreateEntity(jobIndex);

            FsmAction action = new FsmAction {
                stateOwner = stateEntity
            };
            commandBuffer.AddComponent(jobIndex, entity, action);

            return entity;
        }
        
    }
}