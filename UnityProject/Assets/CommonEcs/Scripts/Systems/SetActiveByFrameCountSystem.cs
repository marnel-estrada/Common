using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SetActiveByFrameCountSystem : ISystem {
        private EntityQuery query;
        
        // Handles
        private ComponentTypeHandle<SetActiveByFrameCount> setActiveByFrameCountType;
        private ComponentTypeHandle<Active> activeType;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SetActiveByFrameCount>()
                .WithPresent<Active>()
                .Build(ref state);

            this.setActiveByFrameCountType = state.GetComponentTypeHandle<SetActiveByFrameCount>();
            this.activeType = state.GetComponentTypeHandle<Active>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            this.setActiveByFrameCountType.Update(ref state);
            this.activeType.Update(ref state);
            
            ProcessJob job = new() {
                setActiveByFrameCountType = this.setActiveByFrameCountType,
                activeType = this.activeType
            };
            state.Dependency = job.Schedule(this.query, state.Dependency);
            
        }
        
        [BurstCompile]
        private struct ProcessJob : IJobChunk {
            public ComponentTypeHandle<SetActiveByFrameCount> setActiveByFrameCountType;
            public ComponentTypeHandle<Active> activeType;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<SetActiveByFrameCount> setActiveByFrameCountComponents = chunk.GetNativeArray(ref this.setActiveByFrameCountType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    SetActiveByFrameCount setActiveByFrameCount = setActiveByFrameCountComponents[i];
                    setActiveByFrameCount.frameCountdown -= 1;
                    setActiveByFrameCountComponents[i] = setActiveByFrameCount; // Modify

                    if (setActiveByFrameCount.frameCountdown > 0) {
                        // Frame countdown has not elapsed yet
                        continue;
                    }
                    
                    // Time to activate
                    chunk.SetComponentEnabled(ref this.activeType, i, true);
                    
                    // Deactivate SetActiveByFrameCount so it will not be processed again
                    chunk.SetComponentEnabled(ref this.setActiveByFrameCountType, i, false);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}