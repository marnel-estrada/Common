using CommonEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    public struct PlanRequest : IComponentData {
        // Maps to a GoapAgent entity
        // It has the target goals and fallback goals
        public readonly Entity agentEntity;
        public GoapStatus status;
        
        // The current fallback index
        // Fallback goals are used instead when the goal search fails
        public int fallbackIndex;

        public PlanRequest(Entity agentEntity) {
            this.agentEntity = agentEntity;
            this.status = GoapStatus.RUNNING;
            this.fallbackIndex = -1;
        }

        /// <summary>
        /// Utility method for creating a GoapPlanRequest entity
        /// </summary>
        /// <param name="agentEntity"></param>
        /// <param name="entityManager"></param>
        public static Entity Create(Entity agentEntity, EntityManager entityManager) {
            Entity planRequestEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(planRequestEntity, new PlanRequest(agentEntity));
            
            // Note that the plan request entity also contains the final list of actions to perform
            entityManager.AddBuffer<ActionEntry>(planRequestEntity);
            
            // We created this reference so that plan request entities would be destroyed if the agent entity
            // is destroyed
            EntityReference.Create(agentEntity, planRequestEntity, entityManager);

            return planRequestEntity;
        }

        /// <summary>
        /// Utility method for creating a GoapPlanRequest entity
        /// </summary>
        /// <param name="agentEntity"></param>
        /// <param name="commandBuffer"></param>
        public static Entity Create(Entity agentEntity, EntityCommandBuffer commandBuffer) {
            Entity planRequestEntity = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(planRequestEntity, new PlanRequest(agentEntity));
            
            // Note that the plan request entity also contains the final list of actions to perform
            commandBuffer.AddBuffer<ActionEntry>(planRequestEntity);
            
            // We created this reference so that plan request entities would be destroyed if the agent entity
            // is destroyed
            EntityReference.Create(agentEntity, planRequestEntity, commandBuffer);

            return planRequestEntity;
        }
    }
}