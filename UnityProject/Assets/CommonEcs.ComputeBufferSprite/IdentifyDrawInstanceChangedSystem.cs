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
            
            // Populate index map
            // This is a mapping of the drawInstance entity to its index in the NativeArray that will 
            // represent if something changed to sprites belonging to a draw instance.
            // This used to be implemented as a NativeHashMap. We changed it to NativeArray so we can
            // run it in parallel
            NativeHashMap<Entity, int> ownerToIndexMap = new NativeHashMap<Entity, int>(4, Allocator.Persistent);
            for (int i = 1; i < drawInstances.Count; ++i) {
                ownerToIndexMap.TryAdd(drawInstances[i].Owner, i - 1);
            }
            
            // We minus 1 because the first entry is always the default entry
            int drawInstancesCount = drawInstances.Count - 1;
            NativeArray<bool> transformChangedMap =
                new NativeArray<bool>(drawInstancesCount, Allocator.TempJob);
            
            Job job = new Job() {
                spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>(),
                ownerToIndexMap = ownerToIndexMap,
                transformChangedMap = transformChangedMap
            };
            
            job.Schedule(this.query, inputDeps).Complete();
            
            // Process the results
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                
                // We used OR here because the flags might have been already set to true prior to
                // calling this system
                int changedIndex = ownerToIndexMap[drawInstance.Owner];
                drawInstance.TransformChanged = drawInstance.TransformChanged || transformChangedMap[changedIndex];
            }
            
            // Dispose
            ownerToIndexMap.Dispose();
            transformChangedMap.Dispose();

            return inputDeps;
        }
        
        [BurstCompile]
        private struct Job : IJobChunk {
            [ReadOnly]
            public ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

            [ReadOnly]
            public NativeHashMap<Entity, int> ownerToIndexMap;

            [NativeDisableParallelForRestriction]
            public NativeArray<bool> transformChangedMap;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(this.spriteType);

                for (int i = 0; i < chunk.Count; ++i) {
                    ComputeBufferSprite sprite = sprites[i];
                    int changedIndex = this.ownerToIndexMap[sprite.drawInstanceEntity];
                    
                    if (sprite.transformChanged) {
                        this.transformChangedMap[changedIndex] = true;
                    }
                }
            }
        }
    }
}