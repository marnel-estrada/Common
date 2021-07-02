using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// An interface used to qualify structs as an atom action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAtomActionProcess<T> where T : unmanaged, IAtomActionComponent {
        /// <summary>
        /// Routines before chunk iteration. Calling ArchetypeChunk.GetNativeArray()
        /// can be called here.
        /// </summary>
        /// <param name="batchInChunk"></param>
        /// <param name="batchIndex"></param>
        void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex);

        /// <summary>
        /// Start routines
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <param name="indexOfFirstEntityInQuery"></param>
        /// <param name="iterIndex"></param>
        /// <returns></returns>
        GoapResult Start(ref AtomAction atomAction, ref T actionComponent, int indexOfFirstEntityInQuery, int iterIndex);

        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="atomAction"></param>
        /// <param name="actionComponent"></param>
        /// <param name="indexOfFirstEntityInQuery"></param>
        /// <param name="iterIndex"></param>
        /// <returns></returns>
        GoapResult Update(ref AtomAction atomAction, ref T actionComponent, int indexOfFirstEntityInQuery, int iterIndex);

        /// <summary>
        /// Cleanup routine.
        /// Reverting to previous state when action fails or succeeds can be executed here.
        /// </summary>
        /// <param name="atomAction"></param>
        /// <param name="actionComponent"></param>
        /// <param name="indexOfFirstEntityInQuery"></param>
        /// <param name="iterIndex"></param>
        void Cleanup(ref AtomAction atomAction, ref T actionComponent, int indexOfFirstEntityInQuery, int iterIndex);
    }
}