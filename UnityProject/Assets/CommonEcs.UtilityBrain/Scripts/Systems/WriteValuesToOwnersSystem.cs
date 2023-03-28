using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Writes the computed consideration values to their parent option then each option writes
    /// its value to its parent brain.
    /// </summary>
    [UpdateInGroup(typeof(UtilityBrainSystemGroup))]
    [UpdateBefore(typeof(ResolveBestOptionSystem))]
    public partial class WriteValuesToOwnersSystem : JobSystemBase {
        private EntityQuery considerationsQuery;
        private EntityQuery optionsQuery;

        protected override void OnCreate() {
            this.considerationsQuery = GetEntityQuery(typeof(Consideration));
            this.optionsQuery = GetEntityQuery(typeof(UtilityOption));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            WriteConsiderationsToOptionJob writeConsiderationsToOptions = new() {
                considerationType = GetComponentTypeHandle<Consideration>(true),
                allValueLists = GetBufferLookup<UtilityValue>()
            };
            JobHandle handle = writeConsiderationsToOptions.ScheduleParallel(this.considerationsQuery, inputDeps);

            ComponentTypeHandle<UtilityOption> optionType = GetComponentTypeHandle<UtilityOption>();
            
            ComputeOptionValuesJob computeOptionValues = new() {
                optionType = optionType,
                utilityValueType = GetBufferTypeHandle<UtilityValue>()
            };
            handle = computeOptionValues.ScheduleParallel(this.optionsQuery, handle);

            WriteOptionsToBrainJob writeOptionsToBrain = new() {
                entityType = GetEntityTypeHandle(),
                optionType = optionType,
                allBrainValueBuffers = GetBufferLookup<UtilityValueWithOption>()
            };
            handle = writeOptionsToBrain.ScheduleParallel(this.optionsQuery, handle);
            
            return handle;
        }
        
        [BurstCompile]
        private struct WriteConsiderationsToOptionJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<Consideration> considerationType;

            [NativeDisableParallelForRestriction]
            public BufferLookup<UtilityValue> allValueLists;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Consideration> considerations = chunk.GetNativeArray(ref this.considerationType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Consideration consideration = considerations[i];
                    if (!consideration.shouldExecute) {
                        // Did not execute. No need to write value.
                        continue;
                    }
                    
                    // Set the value
                    DynamicBuffer<UtilityValue> valueList = this.allValueLists[consideration.optionEntity];
                    valueList[consideration.optionIndex] = consideration.value;
                }
            }
        }
        
        // Computes the value for each option. Note that the UtilityValue in UtilityOption is interpreted
        // as maxRank, totalBonus, and final multiplier
        [BurstCompile]
        private struct ComputeOptionValuesJob : IJobChunk {
            public ComponentTypeHandle<UtilityOption> optionType;

            // This is the list of consideration values of each option
            [ReadOnly]
            public BufferTypeHandle<UtilityValue> utilityValueType;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<UtilityOption> options = chunk.GetNativeArray(ref this.optionType);
                BufferAccessor<UtilityValue> considerationValuesList = chunk.GetBufferAccessor(ref this.utilityValueType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    UtilityOption option = options[i];
                    if (!option.shouldExecute) {
                        // Not being executed. No need to continue.
                        continue;
                    }
                    
                    DynamicBuffer<UtilityValue> considerationValues = considerationValuesList[i];
                    option.value = ComputeOptionValue(considerationValues);
                    
                    // Modify
                    options[i] = option;
                }
            }

            private static UtilityValue ComputeOptionValue(in DynamicBuffer<UtilityValue> considerationValues) {
                int maxRank = int.MinValue;
                float totalBonus = 0;
                float multiplier = 1;

                int considerationsLength = considerationValues.Length;
                for (int i = 0; i < considerationsLength; ++i) {
                    UtilityValue current = considerationValues[i];
                    if (current.multiplier.IsZero()) {
                        // This means that the consideration made the option invalid
                        // We can stop the loop right away and return a zero utility value.
                        return new UtilityValue(0, 0, 0);
                    }

                    // Accumulate the values
                    maxRank = math.max(maxRank, current.rank);
                    totalBonus += current.bonus;
                    multiplier *= current.multiplier;
                }
                
                // We use geometric mean so that options with more considerations are scored fairly
                float geometricMean = ComputeGeometricMean(multiplier, considerationsLength);
                return new UtilityValue(maxRank, totalBonus, geometricMean);
            }

            private static float ComputeGeometricMean(float multiplier, int considerationsLength) {
                if (considerationsLength == 0) {
                    // There are no considerations.
                    // This also fixes divide by zero
                    return 0;
                }
                
                return multiplier > 0 ? math.pow(multiplier, 1.0f / considerationsLength) : 0;
            }
        }

        [BurstCompile]
        private struct WriteOptionsToBrainJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            [ReadOnly]
            public ComponentTypeHandle<UtilityOption> optionType;

            // Note here that the value list of UtilityBrain entity is using UtilityValueWithOption 
            [NativeDisableParallelForRestriction]
            public BufferLookup<UtilityValueWithOption> allBrainValueBuffers;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                NativeArray<UtilityOption> options = chunk.GetNativeArray(ref this.optionType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    UtilityOption option = options[i];
                    if (!option.shouldExecute) {
                        // Did not execute. No need to write value.
                        continue;
                    }

                    Entity optionEntity = entities[i];

                    // Write the value
                    DynamicBuffer<UtilityValueWithOption> valueBuffer = this.allBrainValueBuffers[option.utilityBrainEntity];
                    valueBuffer[option.brainIndex] = new UtilityValueWithOption(optionEntity, option.value);
                }
            }
        }
    }
}