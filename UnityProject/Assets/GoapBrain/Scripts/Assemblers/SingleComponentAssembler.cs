using CommonEcs.Goap;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    public abstract class SingleComponentAssembler<T> : GoapAtomAction where T : unmanaged, IComponentData {
        private EntityArchetype archetype;

        public override void Init(ref EntityManager entityManager) {
            this.archetype = entityManager.CreateArchetype(typeof(AtomAction), typeof(T));
        }

        public override void Prepare(ref EntityManager entityManager, in Entity agentEntity, ref NativeList<Entity> linkedEntities) {
            Entity actionEntity = entityManager.CreateEntity(this.archetype);
            PrepareAction(ref entityManager, agentEntity, actionEntity);
            linkedEntities.Add(actionEntity);
        }

        protected virtual void PrepareAction(ref EntityManager entityManager, in Entity agentEntity,
            in Entity actionEntity) {
            // May or may not be overridden by deriving class
        }
    }
}