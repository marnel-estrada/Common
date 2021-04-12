using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    /// <summary>
    /// A class that represents an atomized action
    /// </summary>
    public abstract class AtomActionAssembler {
        /// <summary>
        /// Entity archetype of the atom action can be made here to save garbage when using
        /// EntityManager.CreateEntity(params Type[])
        /// </summary>
        /// <param name="entityManager"></param>
        public abstract void Init(ref EntityManager entityManager);
        
        /// <summary>
        /// Prepares the ECS atom action
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="linkedEntities"></param>
        public abstract void Prepare(ref EntityManager entityManager, in Entity agentEntity,
            ref NativeList<Entity> linkedEntities);
    }
}
