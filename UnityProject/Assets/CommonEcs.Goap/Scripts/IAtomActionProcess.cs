using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// An interface used to qualify structs as an atom action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAtomActionProcess<T> where T : struct, IAtomActionComponent {
        /// <summary>
        /// Routines before chunk iteration. Methods like ArchetypeChunk.GetNativeArray()
        /// can be called here.
        /// </summary>
        /// <param name="chunk"></param>
        void BeforeChunkIteration(ArchetypeChunk chunk);

        /// <summary>
        /// Start routines
        /// </summary>
        /// <param name="atomAction"></param>
        /// <param name="actionComponent"></param>
        /// <param name="chunkIndex"></param>
        /// <param name="queryIndex"></param>
        /// <returns></returns>
        GoapResult Start(ref AtomAction atomAction, ref T actionComponent, int chunkIndex, int queryIndex);

        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="atomAction"></param>
        /// <param name="actionComponent"></param>
        /// <param name="chunkIndex"></param>
        /// <param name="queryIndex"></param>
        /// <returns></returns>
        GoapResult Update(ref AtomAction atomAction, ref T actionComponent, int chunkIndex, int queryIndex);

        /// <summary>
        /// Cleanup routine.
        /// Reverting to previous state when action fails or succeeds can be executed here.
        /// </summary>
        /// <param name="atomAction"></param>
        /// <param name="actionComponent"></param>
        /// <param name="chunkIndex"></param>
        /// <param name="queryIndex"></param>
        void Cleanup(ref AtomAction atomAction, ref T actionComponent, int chunkIndex, int queryIndex);
    }
}