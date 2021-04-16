using CommonEcs.Goap;

using Unity.Entities;

namespace GoapBrain {
    public abstract class SingleComponentActionAssembler<T> : AtomActionAssembler where T : unmanaged, IComponentData {
        private EntityArchetype archetype;

        public override void Init(ref EntityManager entityManager, int actionId, int order) {
            base.Init(ref entityManager, actionId, order);
            this.archetype = entityManager.CreateArchetype(typeof(AtomAction), typeof(T));
        }

        public override Entity Prepare(ref EntityManager entityManager, in Entity agentEntity) {
            Entity actionEntity = entityManager.CreateEntity(this.archetype);
            entityManager.SetComponentData(actionEntity, new AtomAction(this.ActionId, agentEntity, this.Order));
            PrepareAction(ref entityManager, agentEntity, actionEntity);

            return actionEntity;
        }

        protected virtual void PrepareAction(ref EntityManager entityManager, in Entity agentEntity,
            in Entity actionEntity) {
            // May or may not be overridden by deriving class
            // This is needed for cases when the action filter has a custom constructor,
            // or for cases when other components are needed for the action
        }
    }
}