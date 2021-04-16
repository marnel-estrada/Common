using CommonEcs.Goap;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    public abstract class SingleComponentResolverAssembler<T> : ConditionResolverAssembler 
        where T : unmanaged, IComponentData {
        private EntityArchetype archetype;

        public override void Init(ref EntityManager entityManager) {
            this.archetype = entityManager.CreateArchetype(typeof(ConditionResolver), 
                typeof(T));
        }

        /// <summary>
        /// Returns the prepared resolver entity
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="plannerEntity"></param>
        /// <param name="linkedEntities"></param>
        /// <returns></returns>
        public override Entity Prepare(ref EntityManager entityManager, in FixedString64 conditionName, in Entity agentEntity, in Entity plannerEntity) {
            Entity resolverEntity = entityManager.CreateEntity(this.archetype);
            entityManager.SetComponentData(resolverEntity, new ConditionResolver(conditionName, 
                agentEntity, plannerEntity));
            PrepareResolver(ref entityManager, agentEntity, resolverEntity);

            return resolverEntity;
        }

        protected virtual void PrepareResolver(ref EntityManager entityManager, in Entity agentEntity,
            in Entity resolverEntity) {
            // May or may not be overridden by deriving class
            // This is needed for cases when the resolver filter has a custom constructor,
            // or for cases when other components are needed for the action
        }
    }
}