using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(UpdateComputeBufferSpriteTransformSystem))]
    [UpdateAfter(typeof(CopyTrsFromGameObjectToComputeBufferSpriteSystem))]
    [UpdateBefore(typeof(UpdateDrawInstanceArraysSystem))]
    public class IdentifyDrawInstanceChangedSystem : SystemBase {
        private EntityQuery query;
        private SharedComponentQuery<ComputeBufferDrawInstance> drawInstanceQuery;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ComputeBufferSprite), typeof(ComputeBufferDrawInstance));
            this.drawInstanceQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            this.drawInstanceQuery.Update();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.drawInstanceQuery.SharedComponents;
            
            // We minus 1 because the first entry is always the default entry
            int drawInstancesCount = drawInstances.Count - 1;
            NativeHashMap<Entity, byte> transformChangedMap =
                new NativeHashMap<Entity, byte>(drawInstancesCount, Allocator.TempJob);
            
            Job job = new Job() {
                spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>(),
                chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob),
                transformChangedMap = transformChangedMap
            };
            
            job.Schedule(inputDeps).Complete();
            
            // Process the results
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                
                // Note that we're only checking for existence here
                // We used OR here because the flags might have been already set to true prior to
                // calling this system
                drawInstance.TransformChanged = drawInstance.TransformChanged || transformChangedMap.TryGetValue(drawInstance.Owner, out byte _);
            }
            
            // Dispose
            transformChangedMap.Dispose();

            return inputDeps;
        }
        
        [BurstCompile]
        private struct Job : IJob {
            [ReadOnly]
            public ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;
            
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;

            public NativeHashMap<Entity, byte> transformChangedMap;
            
            public void Execute() {
                for (int i = 0; i < this.chunks.Length; ++i) {
                    ArchetypeChunk chunk = this.chunks[i];
                    Process(ref chunk);
                }
            }

            private void Process(ref ArchetypeChunk chunk) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(this.spriteType);

                for (int i = 0; i < chunk.Count; ++i) {
                    ComputeBufferSprite sprite = sprites[i];
                    if (sprite.transformChanged) {
                        this.transformChangedMap.TryAdd(sprite.drawInstanceEntity, 0);
                    }
                }
            }
        }
    }
}