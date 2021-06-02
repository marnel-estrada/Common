using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public interface IFsmActionExecution<T> where T : struct, IFsmActionComponent {
        /// <summary>
        /// Routines before chunk iteration. Calling ArchetypeChunk.GetNativeArray()
        /// can be called here.
        /// </summary>
        /// <param name="batchInChunk"></param>
        /// <param name="batchIndex"></param>
        void BeforeChunkIteration(ArchetypeChunk batchInChunk, int batchIndex);
        
        /// <summary>
        /// DotsFsm is passed here so that the action can send events
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        void OnEnter(Entity actionEntity, ref DotsFsmAction action, ref T actionComponent, int indexOfFirstEntityInQuery, int iterIndex);
        
        /// <summary>
        /// DotsFsm is passed here so that the action can send events
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        void OnUpdate(Entity actionEntity, ref DotsFsmAction action, ref T actionComponent, int indexOfFirstEntityInQuery, int iterIndex);
        
        void OnExit(Entity actionEntity, DotsFsmAction action, ref T actionComponent, int indexOfFirstEntityInQuery, int iterIndex);
    }
}