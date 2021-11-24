using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// We implemented in separate file so we don't need to add in AssemblyInfo.
    /// </summary>
    [BurstCompile]
    public struct CollectActionsThatCanExecuteJob : IJobEntityBatch {
        [ReadOnly]
        public EntityTypeHandle entityType;

        [ReadOnly]
        public ComponentTypeHandle<AtomAction> atomActionType;

        [ReadOnly]
        public ComponentDataFromEntity<DebugEntity> allDebugEntities;
            
        public NativeList<Entity>.ParallelWriter resultList;
            
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
            NativeArray<Entity> entities = batchInChunk.GetNativeArray(this.entityType);
            NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);

            for (int i = 0; i < batchInChunk.Count; ++i) {
                AtomAction atomAction = atomActions[i];
                if (atomAction.canExecute) {
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
}