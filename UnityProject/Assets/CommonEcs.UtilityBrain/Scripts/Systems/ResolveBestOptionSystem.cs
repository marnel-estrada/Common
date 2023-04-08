using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    [UpdateInGroup(typeof(UtilityBrainSystemGroup))]
    public partial class ResolveBestOptionSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(UtilityBrain));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ResolveBestOptionJob resolveBestOptionJob = new() {
                brainType = GetComponentTypeHandle<UtilityBrain>(),
                valueBufferType = GetBufferTypeHandle<UtilityValueWithOption>(),
                allDebug = GetComponentLookup<DebugEntity>()
            };
            return resolveBestOptionJob.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct ResolveBestOptionJob : IJobChunk {
            public ComponentTypeHandle<UtilityBrain> brainType;
            
            [ReadOnly]
            public BufferTypeHandle<UtilityValueWithOption> valueBufferType;

            [ReadOnly]
            public ComponentLookup<DebugEntity> allDebug;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<UtilityBrain> brains = chunk.GetNativeArray(ref this.brainType);
                BufferAccessor<UtilityValueWithOption> valuesList = chunk.GetBufferAccessor(ref this.valueBufferType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    UtilityBrain brain = brains[i];
                    if (!brain.shouldExecute) {
                        // It did not execute. Skip.
                        continue;
                    }

                    if (this.allDebug[brain.agentEntity].enabled) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }

                    DynamicBuffer<UtilityValueWithOption> values = valuesList[i];
                    brain.currentBestOption = ResolveBestOption(values);
                    
                    // We also reset shouldExecute here as the best option is already resolved
                    brain.shouldExecute = false;
                    
                    // Modify
                    brains[i] = brain;
                }
            }

            // Note here that we don't need to sort the values if we just want the best option
            // We can just traverse them and compare like trying to resolve the max value
            private static ValueTypeOption<Entity> ResolveBestOption(in DynamicBuffer<UtilityValueWithOption> values) {
                ValueTypeOption<Entity> bestOption = ValueTypeOption<Entity>.None;
                int bestRank = int.MinValue; // Rank of the best option
                float bestWeight = float.MinValue; // Weight of the best option 
                
                for (int i = 0; i < values.Length; ++i) {
                    UtilityValueWithOption current = values[i];
                    UtilityValue currentValue = current.value;
                    if (currentValue.multiplier.IsZero()) {
                        // Skip options that has zero multiplier
                        // It means that they are not to be selected
                        continue;
                    }
                    
                    if (IsBetterOption(current, bestRank, bestWeight)) {
                        // Found a better option
                        bestRank = currentValue.rank;
                        bestWeight = currentValue.Weight;
                        bestOption = ValueTypeOption<Entity>.Some(current.optionEntity);
                    }
                }
                
                return bestOption;
            }

            private static bool IsBetterOption(in UtilityValueWithOption utilValue, int bestRank,
                float bestWeight) {
                if (utilValue.value.rank > bestRank) {
                    // We have found a new better option
                    return true;
                }
                    
                if (utilValue.value.rank == bestRank) {
                    // Equal rank. Let's check weight instead.
                    return utilValue.value.Weight > bestWeight;
                }

                return false;
            }
        }
    }
}