using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// An interface used to qualify structs as an atom action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAtomActionProcess<T> where T : unmanaged, IComponentData {
        /// <summary>
        /// Start routines
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <returns></returns>
        GoapResult Start(ref AtomAction atomAction, ref T actionComponent);
        
        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <returns></returns>
        GoapResult Update(ref AtomAction atomAction, ref T actionComponent);
    }
}