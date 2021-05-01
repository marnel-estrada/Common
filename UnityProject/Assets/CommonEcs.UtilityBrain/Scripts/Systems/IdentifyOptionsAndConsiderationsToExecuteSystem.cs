using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// Identifies which options and considerations can run by setting their shouldExecute value.
    /// The value is whatever their parent UtilityBrain value is (when its shouldExecute is true).
    /// </summary>
    public class IdentifyOptionsAndConsiderationsToExecuteSystem : JobSystemBase {
        private EntityQuery optionsQuery;
        private EntityQuery considerationsQuery;

        protected override void OnCreate() {
            this.optionsQuery = GetEntityQuery(typeof(Option));
            this.considerationsQuery = GetEntityQuery(typeof(Consideration));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            IdentifyOptionsJob identifyOptionsJob = new IdentifyOptionsJob() {
                optionType = GetComponentTypeHandle<Option>(), 
                allBrains = GetComponentDataFromEntity<UtilityBrain>()
            };
            JobHandle handle = identifyOptionsJob.ScheduleParallel(this.optionsQuery, 1, inputDeps);

            IdentifyConsiderationsJob identifyConsiderationsJob = new IdentifyConsiderationsJob() {
                considerationType = GetComponentTypeHandle<Consideration>(),
                allOptions = GetComponentDataFromEntity<Option>()
            };
            handle = identifyConsiderationsJob.ScheduleParallel(this.considerationsQuery, 1, handle);
            
            return handle;
        }
        
        private struct IdentifyOptionsJob : IJobEntityBatch {
            public ComponentTypeHandle<Option> optionType;

            [ReadOnly]
            public ComponentDataFromEntity<UtilityBrain> allBrains;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Option> options = batchInChunk.GetNativeArray(this.optionType);
                for (int i = 0; i < options.Length; ++i) {
                    Option option = options[i];
                    option.shouldExecute = this.allBrains[option.utilityBrainEntity].shouldExecute;
                    
                    // Modify
                    options[i] = option;
                }   
            }
        }

        private struct IdentifyConsiderationsJob : IJobEntityBatch {
            public ComponentTypeHandle<Consideration> considerationType;

            [ReadOnly]
            public ComponentDataFromEntity<Option> allOptions;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Consideration> considerations = batchInChunk.GetNativeArray(this.considerationType);
                for (int i = 0; i < considerations.Length; ++i) {
                    Consideration consideration = considerations[i];
                    consideration.shouldExecute = this.allOptions[consideration.optionEntity].shouldExecute;
                    
                    // Modify
                    considerations[i] = consideration;
                }
            }
        }
    }
}