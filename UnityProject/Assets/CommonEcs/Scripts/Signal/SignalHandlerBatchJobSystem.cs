using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// Handles the signals by batches. This is different from SignalHandlerJobComponentSystem which handles
    /// signals one after another.
    /// </summary>
    /// <typeparam name="TParameterType"></typeparam>
    /// <typeparam name="TProcessorType"></typeparam>
    public abstract partial class SignalHandlerBatchJobSystem<TParameterType, TProcessorType> : JobSystemBase 
        where TParameterType : unmanaged, IComponentData
        where TProcessorType : unmanaged, ISignalProcessor<TParameterType> {
        private EntityQuery query;
        private EndInitializationEntityCommandBufferSystem commandBufferSystem;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Signal), typeof(TParameterType), ComponentType.Exclude<ProcessedBySystem>());;
            this.commandBufferSystem = this.World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ProcessJob job = new ProcessJob() {
                entityType = GetEntityTypeHandle(),
                parameterType = GetComponentTypeHandle<TParameterType>(true),
                processor = PrepareProcessor(),
                commandBuffer = this.commandBufferSystem.CreateCommandBuffer()
            };
            
            // We don't use ScheduleParallel() since the processor might have used ComponentLookup
            // or BufferLookup
            JobHandle handle = job.Schedule(this.query, inputDeps);
            
            this.commandBufferSystem.AddJobHandleForProducer(handle);
            
            return handle;
        }

        [BurstCompile]
        public struct ProcessJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            [ReadOnly]
            public ComponentTypeHandle<TParameterType> parameterType;

            public TProcessorType processor;

            public EntityCommandBuffer commandBuffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                NativeArray<TParameterType> parameters = chunk.GetNativeArray(ref this.parameterType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Entity entity = entities[i];
                    this.processor.Execute(entity, parameters[i]);
                    
                    // We added this component so that they will not be processed again
                    this.commandBuffer.AddComponent<ProcessedBySystem>(entity);
                }
            }
        }

        protected abstract TProcessorType PrepareProcessor();
        
        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }
    }
}