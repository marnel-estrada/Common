using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    /// <summary>
    /// An abstract base class for classes that resolves a condition value
    /// </summary>
    public abstract class ConditionResolverAssembler {
        /// <summary>
        /// Entity archetype of the atom action can be made here to save garbage when using
        /// EntityManager.CreateEntity(params Type[])
        /// </summary>
        /// <param name="entityManager"></param>
        public abstract void Init(ref EntityManager entityManager);
        
        /// <summary>
        /// Prepares the ECS condition resolver
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="linkedEntities"></param>
        public abstract Entity Prepare(ref EntityManager entityManager, in FixedString64 conditionName, in Entity agentEntity, in Entity plannerEntity);
    }
}