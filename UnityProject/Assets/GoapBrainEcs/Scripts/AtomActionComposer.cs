using Unity.Entities;

namespace GoapBrainEcs {
    /// <summary>
    /// We made it into an abstract class to make clear the intent that implementing classes must be
    /// their own separate class instead of piggybacking from large classes like systems if we had made it
    /// as an interface.
    /// </summary>
    public abstract class AtomActionComposer {
        /// <summary>
        /// Prepares the atom action by adding components to it or creating other entities that the
        /// action needs to execute
        /// </summary>
        /// <param name="atomActionEntity"></param>
        /// <param name="commandBuffer"></param>
        public abstract void Prepare(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer);

        /// <summary>
        /// Returns whether or not the action does something on fail
        /// This is used to skip an atom action if it doesn't have an action to do on fail
        /// </summary>
        public abstract bool HasOnFailAction { get; }

        /// <summary>
        /// Prepares the entity that may be executed later if the whole action failed
        /// </summary>
        /// <param name="atomActionEntity"></param>
        /// <param name="commandBuffer"></param>
        public abstract void PrepareOnFailAction(Entity agentEntity, Entity atomActionEntity, EntityCommandBuffer commandBuffer);
    }
}