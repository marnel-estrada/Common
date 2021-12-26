using Unity.Burst;
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
    public class WriteValuesToOwnersSystem : JobSystemBase {
        private EntityQuery considerationsQuery;
        private EntityQuery optionsQuery;

        protected override void OnCreate() {
            this.considerationsQuery = GetEntityQuery(typeof(Consideration));
            this.optionsQuery = GetEntityQuery(typeof(UtilityOption));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            WriteConsiderationsToOptionJob writeConsiderationsToOptions = new WriteConsiderationsToOptionJob() {
                considerationType = GetComponentTypeHandle<Consideration>(true),
                allValueLists = GetBufferFromEntity<UtilityValue>()
            };
            JobHandle handle = writeConsiderationsToOptions.ScheduleParallel(this.considerationsQuery, 1, inputDeps);

            ComponentTypeHandle<UtilityOption> optionType = GetComponentTypeHandle<UtilityOption>();
            
            ComputeOptionValuesJob computeOptionValues = new ComputeOptionValuesJob() {
                optionType = optionType,
                utilityValueType = GetBufferTypeHandle<UtilityValue>()
            };
            handle = computeOptionValues.ScheduleParallel(this.optionsQuery, 1, handle);

            WriteOptionsToBrainJob writeOptionsToBrain = new WriteOptionsToBrainJob() {
                entityType = GetEntityTypeHandle(),
                optionType = optionType,
                allBrainValueBuffers = GetBufferFromEntity<UtilityValueWithOption>()
            };
            handle = writeOptionsToBrain.ScheduleParallel(this.optionsQuery, 1, handle);
            
            return handle;
        }
        
        [BurstCompile]
        private struct WriteConsiderationsToOptionJob : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<Consideration> considerationType;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<UtilityValue> allValueLists;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Consideration> considerations = batchInChunk.GetNativeArray(this.considerationType);
                
                for (int i = 0; i < considerations.Length; ++i) {
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
        private struct ComputeOptionValuesJob : IJobEntityBatch {
            public ComponentTypeHandle<UtilityOption> optionType;

            // This is the list of consideration values of each option
            [ReadOnly]
            public BufferTypeHandle<UtilityValue> utilityValueType;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<UtilityOption> options = batchInChunk.GetNativeArray(this.optionType);
                BufferAccessor<UtilityValue> considerationValuesList = batchInChunk.GetBufferAccessor(this.utilityValueType);
                
                for (int i = 0; i < batchInChunk.Count; ++i) {
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
        private struct WriteOptionsToBrainJob : IJobEntityBatch {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            [ReadOnly]
            public ComponentTypeHandle<UtilityOption> optionType;

            // Note here that the value list of UtilityBrain entity is using UtilityValueWithOption 
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<UtilityValueWithOption> allBrainValueBuffers; 
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Entity> entities = batchInChunk.GetNativeArray(this.entityType);
                NativeArray<UtilityOption> options = batchInChunk.GetNativeArray(this.optionType);
                for (int i = 0; i < options.Length; ++i) {
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