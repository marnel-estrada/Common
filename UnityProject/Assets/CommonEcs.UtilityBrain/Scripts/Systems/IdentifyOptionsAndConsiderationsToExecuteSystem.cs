using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Identifies which options and considerations can run by setting their shouldExecute value.
    /// The value is whatever their parent UtilityBrain value is (when its shouldExecute is true).
    /// </summary>
    [UpdateInGroup(typeof(UtilityBrainSystemGroup))]
    public partial class IdentifyOptionsAndConsiderationsToExecuteSystem : JobSystemBase {
        private EntityQuery optionsQuery;
        private EntityQuery considerationsQuery;

        protected override void OnCreate() {
            this.optionsQuery = GetEntityQuery(typeof(UtilityOption));
            this.considerationsQuery = GetEntityQuery(typeof(Consideration));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            IdentifyOptionsJob identifyOptionsJob = new() {
                optionType = GetComponentTypeHandle<UtilityOption>(), 
                allBrains = GetComponentLookup<UtilityBrain>()
            };
            JobHandle handle = identifyOptionsJob.ScheduleParallel(this.optionsQuery, inputDeps);

            IdentifyConsiderationsJob identifyConsiderationsJob = new() {
                considerationType = GetComponentTypeHandle<Consideration>(),
                allOptions = GetComponentLookup<UtilityOption>()
            };
            handle = identifyConsiderationsJob.ScheduleParallel(this.considerationsQuery, handle);
            
            return handle;
        }
        
        [BurstCompile]
        private struct IdentifyOptionsJob : IJobChunk {
            public ComponentTypeHandle<UtilityOption> optionType;

            [ReadOnly]
            public ComponentLookup<UtilityBrain> allBrains;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<UtilityOption> options = chunk.GetNativeArray(ref this.optionType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    UtilityOption option = options[i];
                    option.shouldExecute = this.allBrains[option.utilityBrainEntity].shouldExecute;
                    
                    // Modify
                    options[i] = option;
                }
            }
        }

        [BurstCompile]
        private struct IdentifyConsiderationsJob : IJobChunk {
            public ComponentTypeHandle<Consideration> considerationType;

            [ReadOnly]
            public ComponentLookup<UtilityOption> allOptions;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Consideration> considerations = chunk.GetNativeArray(ref this.considerationType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Consideration consideration = considerations[i];
                    consideration.shouldExecute = this.allOptions[consideration.optionEntity].shouldExecute;
                    
                    // Modify
                    considerations[i] = consideration;
                }
            }
        }
    }
}