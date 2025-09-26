using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// We implemented in separate file so we don't need to add in AssemblyInfo.
    /// </summary>
    [BurstCompile]
    public struct CollectActionEntities : IJobChunk {
        [ReadOnly]
        public EntityTypeHandle entityType;

        [ReadOnly]
        public ComponentTypeHandle<AtomAction> atomActionType;
        
        [ReadOnly]
        public ComponentLookup<GoapAgent> allAgents;

        [ReadOnly]
        public ComponentLookup<DebugEntity> allDebugEntities;

        public NativeList<Entity>.ParallelWriter cleanupResults;
        public NativeList<Entity>.ParallelWriter canExecuteResults;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<AtomAction> atomActions = chunk.GetNativeArray(ref this.atomActionType);

            ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (enumerator.NextEntityIndex(out int i)) {
                AtomAction atomAction = atomActions[i];
                GoapAgent agent = this.allAgents[atomAction.agentEntity];
                
                if (agent.state == AgentState.CLEANUP) {
                    // Add to cleanup list so it can be invoked outside of this job
                    this.cleanupResults.AddNoResize(entities[i]);
                    continue;
                }
                
                if (!atomAction.canExecute) {
                    continue;
                }
                
                DebugEntity debug = this.allDebugEntities[atomAction.agentEntity];
                if (debug.enabled) {
                    int breakpoint = 0;
                    ++breakpoint;
                }
                    
                this.canExecuteResults.AddNoResize(entities[i]);
            }
        }
    }
}