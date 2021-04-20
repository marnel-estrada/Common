﻿using Unity.Entities;

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
        /// Prepares the ECS atom action
        /// The entity returned here is the action entity
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="agentEntity"></param>
        /// <param name="linkedEntities"></param>
        public abstract Entity Prepare(ref EntityManager entityManager, in Entity agentEntity);
        
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