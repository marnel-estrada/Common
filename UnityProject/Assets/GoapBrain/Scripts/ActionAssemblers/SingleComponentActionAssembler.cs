using CommonEcs.Goap;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    public abstract class SingleComponentActionAssembler<T> : AtomActionAssembler where T : unmanaged, IAtomActionComponent {
        private EntityArchetype archetype;

        public override void Init(ref EntityManager entityManager, int actionId, int order) {
            base.Init(ref entityManager, actionId, order);
            this.archetype = entityManager.CreateArchetype(
                typeof(AtomAction), typeof(AtomAction.CanExecute), typeof(T));
        }

        public override void Prepare(ref EntityManager entityManager, in Entity agentEntity, ref NativeList<Entity> linkedEntities) {
            Entity actionEntity = entityManager.CreateEntity(this.archetype);
            entityManager.SetComponentData(actionEntity, new AtomAction(this.ActionId, agentEntity, this.Order));
            
            PrepareActionComponent(ref entityManager, agentEntity, actionEntity);
            
            linkedEntities.Add(actionEntity);
        }

        protected virtual void PrepareActionComponent(ref EntityManager entityManager, in Entity agentEntity,
            in Entity actionEntity) {
            // May or may not be overridden by deriving class
            // This is needed for cases when the action filter has a custom constructor,
            // or for cases when other components are needed for the action
        }
    }
}