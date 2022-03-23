using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// Handles the signals by batches. This is different from SignalHandlerJobComponentSystem which handles
    /// signals one after another.
    /// </summary>
    /// <typeparam name="ParameterType"></typeparam>
    /// <typeparam name="ProcessorType"></typeparam>
    public abstract partial class SignalHandlerBatchJobSystem<ParameterType, ProcessorType> : JobSystemBase 
        where ParameterType : struct, IComponentData
        where ProcessorType : struct, ISignalProcessor<ParameterType> {
        private EntityQuery query;
        private EndInitializationEntityCommandBufferSystem barrier;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Signal), typeof(ParameterType), ComponentType.Exclude<ProcessedBySystem>());;
            this.barrier = this.World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ProcessJob job = new ProcessJob() {
                entityType = GetEntityTypeHandle(),
                parameterType = GetComponentTypeHandle<ParameterType>(true),
                processor = PrepareProcessor(),
                commandBuffer = this.barrier.CreateCommandBuffer()
            };
            
            // We don't use ScheduleParallel() since the processor might have used ComponentDataFromEntity
            // or BufferFromEntity
            JobHandle handle = job.Schedule(this.query, inputDeps);
            
            this.barrier.AddJobHandleForProducer(handle);
            
            return handle;
        }

        [BurstCompile]
        public struct ProcessJob : IJobEntityBatch {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            [ReadOnly]
            public ComponentTypeHandle<ParameterType> parameterType;

            public ProcessorType processor;

            public EntityCommandBuffer commandBuffer;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Entity> entities = batchInChunk.GetNativeArray(this.entityType);
                NativeArray<ParameterType> parameters = batchInChunk.GetNativeArray(this.parameterType);
                for (int i = 0; i < batchInChunk.Count; ++i) {
                    Entity entity = entities[i];
                    this.processor.Execute(entity, parameters[i]);
                    
                    // We added this component so that they will not be processed again
                    this.commandBuffer.AddComponent<ProcessedBySystem>(entity);
                }
            }
        }

        protected abstract ProcessorType PrepareProcessor();
        
        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }
    }
}