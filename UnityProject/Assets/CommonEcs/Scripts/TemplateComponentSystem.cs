using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A common system with template methods that we can use to simplify writing systems
    /// </summary>
    public abstract class TemplateComponentSystem : SystemBase {
        private EntityQuery query;
        
        protected override void OnCreate() {
            this.query = ComposeQuery();
        }

        /// <summary>
        /// Let's deriving class define the group that the system cares about
        /// </summary>
        /// <returns></returns>
        protected abstract EntityQuery ComposeQuery();
        
        protected override void OnUpdate() {
            BeforeChunkTraversal();
            
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        /// <summary>
        /// Routines before update. Caching of ArchetypeChunk*Types are done here
        /// </summary>
        protected abstract void BeforeChunkTraversal();
        
        private void Process(ArchetypeChunk chunk) {
            BeforeProcessChunk(chunk);

            for (int i = 0; i < chunk.Count; ++i) {
                Process(i);
            }
        }

        /// <summary>
        /// Rotuines before processing the chunk. Usually the caching of NativeArrays are done here
        /// </summary>
        /// <param name="chunk"></param>
        protected abstract void BeforeProcessChunk(ArchetypeChunk chunk);

        /// <summary>
        /// Processes the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        protected abstract void Process(int index);
    }
}