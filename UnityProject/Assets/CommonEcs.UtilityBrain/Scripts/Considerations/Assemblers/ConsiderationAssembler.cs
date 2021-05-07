using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    public abstract class ConsiderationAssembler {
        public virtual void Init(ref EntityManager entityManager) {
        }

        public abstract Entity Prepare(ref EntityManager entityManager, in Entity agentEntity, in Entity optionEntity, 
            int optionIndex, ref NativeList<Entity> linkedEntities);
    }
}