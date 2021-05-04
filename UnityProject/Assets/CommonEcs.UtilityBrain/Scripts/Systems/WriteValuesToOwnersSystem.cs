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
            
            ComputeOptionValuesJob computeOptionValues = new ComputeOptionValuesJob() {
                optionType = GetComponentTypeHandle<UtilityOption>(),
                utilityValueType = GetBufferTypeHandle<UtilityValue>()
            };
            handle = computeOptionValues.ScheduleParallel(this.optionsQuery, 1, handle);

            WriteOptionsToBrainJob writeOptionsToBrain = new WriteOptionsToBrainJob() {
                optionType = GetComponentTypeHandle<UtilityOption>(),
                allBuckets = GetBufferFromEntity<DynamicBufferHashMap<OptionId, UtilityValue>.Entry<UtilityValue>>()
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
                
                for (int i = 0; i < considerationValues.Length; ++i) {
                    UtilityValue current = considerationValues[i];
                    if (Comparison.IsZero(current.multiplier)) {
                        // This means that the consideration made the option invalid
                        // We can stop the loop right away and return a zero utility value.
                        return new UtilityValue(0, 0, 0);
                    }

                    // Accumulate the values
                    maxRank = math.max(maxRank, current.rank);
                    totalBonus += current.bonus;
                    multiplier *= current.multiplier;
                }

                return new UtilityValue(maxRank, totalBonus, multiplier);
            }
        }

        [BurstCompile]
        private struct WriteOptionsToBrainJob : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<UtilityOption> optionType;

            // Note here that we write to the bucket of the parent UtilityBrain
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<DynamicBufferHashMap<OptionId, UtilityValue>.Entry<UtilityValue>> allBuckets; 
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<UtilityOption> options = batchInChunk.GetNativeArray(this.optionType);
                for (int i = 0; i < options.Length; ++i) {
                    UtilityOption option = options[i];
                    if (!option.shouldExecute) {
                        // Did not execute. No need to write value.
                        continue;
                    }

                    // Write the value
                    DynamicBuffer<DynamicBufferHashMap<OptionId, UtilityValue>.Entry<UtilityValue>> bucket = this.allBuckets[option.utilityBrainEntity];
                    DynamicBufferHashMap<OptionId, UtilityValue>.Entry<UtilityValue> entry = bucket[option.brainIndex];
                    bucket[option.brainIndex] = DynamicBufferHashMap<OptionId, UtilityValue>.Entry<UtilityValue>.Something(entry.HashCode, option.value);
                }
            }
        }
    }
}