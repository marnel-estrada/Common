using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(IdentifyDrawInstanceChangedSystem))]
    public class UpdateChangedSpritesMasterListSystem : SystemBase {
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

            JobHandle handle = inputDeps;
            ArchetypeChunkComponentType<ComputeBufferSprite> spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>();

            // Note that we always start from 1 when working with shared components
            // The first one is always blank
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                if (!drawInstance.SomethingChanged) {
                    // We can skip because nothing changed
                    continue;
                }
                
                this.query.SetSharedComponentFilter(drawInstance);
                
                // Schedule job that will update the sprite in master list
                handle = new Job() {
                    spriteType = spriteType,
                    masterList = drawInstance.SpritesMasterList
                }.ScheduleParallel(this.query, handle);
            }
            
            return handle;
        }
        
        [BurstCompile]
        private struct Job : IJobChunk {
            [ReadOnly]
            public ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> masterList;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(this.spriteType);
                for (int i = 0; i < sprites.Length; ++i) {
                    ComputeBufferSprite sprite = sprites[i];
                    if (!sprite.SomethingChanged) {
                        // Skip as nothing changed
                        continue;
                    }

                    // Update the sprite in master list
                    this.masterList[sprite.masterListIndex] = sprite;
                }
            }
        }
    }
}