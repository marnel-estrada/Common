using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public abstract class SingleComponentConsiderationAssembler<T> : ConsiderationAssembler
        where T : unmanaged, IConsiderationComponent {
        private EntityArchetype archetype;

        public override void Init(ref EntityManager entityManager) {
            this.archetype = entityManager.CreateArchetype(typeof(Consideration), typeof(T));
        }

        public override void Prepare(ref EntityManager entityManager, in Entity agentEntity, in Entity optionEntity, 
            int optionIndex, ref NativeList<Entity> linkedEntities) {
            Entity considerationEntity = entityManager.CreateEntity(this.archetype);
            entityManager.SetComponentData(considerationEntity, new Consideration(agentEntity, optionEntity, optionIndex));
            
            linkedEntities.Add(considerationEntity);
        }

        protected virtual void PrepareConsiderationComponent(ref EntityManager entityManager, in Entity agentEntity,
            in Entity considerationEntity) {
            // May or may not be overridden by deriving class
            // This is needed for cases when the consideration filter has a custom constructor,
            // or for cases when other components are needed for the action
            // or for cases when the consideration requires other entities to work
        }
    }
}