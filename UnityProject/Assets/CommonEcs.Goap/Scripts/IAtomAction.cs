using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// An interface used to qualify structs as an atom action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAtomAction<T> where T : struct, IComponentData {
        /// <summary>
        /// Start routines
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <returns></returns>
        GoapResult Start(Entity agentEntity, Entity actionEntity, ref T actionComponent);
        
        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <returns></returns>
        GoapResult Update(Entity agentEntity, Entity actionEntity, ref T actionComponent);
    }
}