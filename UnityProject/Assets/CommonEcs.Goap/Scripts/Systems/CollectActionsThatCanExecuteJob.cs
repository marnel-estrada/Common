using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// We implemented in separate file so we don't need to add in AssemblyInfo.
    /// </summary>
    [BurstCompile]
    public struct CollectActionsThatCanExecuteJob : IJobChunk {
        [ReadOnly]
        public EntityTypeHandle entityType;

        [ReadOnly]
        public ComponentTypeHandle<AtomAction> atomActionType;

        [ReadOnly]
        public ComponentLookup<DebugEntity> allDebugEntities;
            
        public NativeList<Entity>.ParallelWriter resultList;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<AtomAction> atomActions = chunk.GetNativeArray(ref this.atomActionType);

            ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (enumerator.NextEntityIndex(out int i)) {
                AtomAction atomAction = atomActions[i];
                
                // Assumes that the query already excludes atom actions that can't execute
                // if (!atomAction.canExecute) {
                //     continue;
                // }
                
                DebugEntity debug = this.allDebugEntities[atomAction.agentEntity];
                if (debug.enabled) {
                    int breakpoint = 0;
                    ++breakpoint;
                }
                    
                this.resultList.AddNoResize(entities[i]);
            }
        }
    }
}