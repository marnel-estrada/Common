using Unity.Collections;
using Unity.Entities;

namespace GoapBrain {
    /// <summary>
    /// A class that represents an atomized action
    /// </summary>
    public abstract class AtomActionAssembler {
        private int actionId;
        private int order;

        /// <summary>
        /// Entity archetype of the atom action can be made here to save garbage when using
        /// EntityManager.CreateEntity(params Type[])
        /// </summary>
        /// <param name="entityManager"></param>
        public virtual void Init(ref EntityManager entityManager, int actionId, int order) {
            this.actionId = actionId;
            this.order = order;
        }
        
        /// <summary>
        /// Prepares an atom action.
        ///
        /// We need to pass the list of linkedEntities here for cases where a GOAP action might prepare
        /// an FSM in its execution.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="linkedEntities"></param>
        public abstract void Prepare(ref EntityManager entityManager, in Entity agentEntity, ref NativeList<Entity> linkedEntities);
        
        protected int ActionId {
            get {
                return this.actionId;
            }
        }

        protected int Order {
            get {
                return this.order;
            }
        }
    }
}
