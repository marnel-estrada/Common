using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformGameObjectSpriteVerticesSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UseYAsSortOrderGameObjectSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(
                typeof(Sprite), 
                typeof(Transform),
                ComponentType.ReadOnly<UseYAsSortOrder>(),
                ComponentType.Exclude<Static>()
            );
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            TransformAccessArray transforms = this.query.GetTransformAccessArray();
            NativeArray<TransformStash> stashes = new NativeArray<TransformStash>(transforms.length, Allocator.TempJob);
            
            // Job for copying to stashes
            StashTransformsJob stashTransforms = new StashTransformsJob() {
                stashes = stashes
            };
            JobHandle stashHandle = stashTransforms.Schedule(transforms, inputDeps);
            
            // Job for applying to sprites
            UpdateSpritesJob updateSpritesJob = new UpdateSpritesJob() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                useYAsSortOrderType = GetComponentTypeHandle<UseYAsSortOrder>(),
                stashes = stashes
            };

            return updateSpritesJob.ScheduleParallel(this.query, stashHandle);
        }
        
        [BurstCompile]
        private struct UpdateSpritesJob : IJobEntityBatchWithIndex {
            public ComponentTypeHandle<Sprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<UseYAsSortOrder> useYAsSortOrderType;
            
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<TransformStash> stashes;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<Sprite> sprites = batchInChunk.GetNativeArray(this.spriteType);
                NativeArray<UseYAsSortOrder> sortOrders = batchInChunk.GetNativeArray(this.useYAsSortOrderType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    Sprite sprite = sprites[i];
                    UseYAsSortOrder sortOrder = sortOrders[i];
                    int index = indexOfFirstEntityInQuery + i;
                    
                    float3 position = this.stashes[index].position;
                
                    // We use negative of z here because the higher z should be ordered first
                    sprite.RenderOrder = -(position.y + sortOrder.offset);
                    
                    // Modify Sprite
                    sprites[i] = sprite;
                }
            }
        }
    }
}
