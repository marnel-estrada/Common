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
    [UpdateBefore(typeof(EndPresentationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UseYAsSortOrderGameObjectSystem : JobComponentSystem {
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
                stashes = stashes
            };

            return updateSpritesJob.Schedule(this.query, stashHandle);
        }
        
        [BurstCompile]
        private struct UpdateSpritesJob : IJobForEachWithEntity<Sprite, UseYAsSortOrder> {
            [DeallocateOnJobCompletion]
            public NativeArray<TransformStash> stashes;

            public void Execute(Entity entity, int index, ref Sprite sprite, [ReadOnly] ref UseYAsSortOrder sortOrder) {
                float3 position = this.stashes[index].position;
                
                // We use negative of z here because the higher z should be ordered first
                sprite.RenderOrder = -(position.y + sortOrder.offset);
            }
        }
    }
}
