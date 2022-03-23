using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    /// <summary>
    /// This is the same as SpriteManagerTransformSystem but for sprite made in GameObject world
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class TransformGameObjectSpriteVerticesSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            // All entities with Sprite and Transform, but without Static (non Static sprites)
            this.query = GetEntityQuery(typeof(Sprite), typeof(Transform), ComponentType.Exclude<Static>());
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
            ApplyTransformsJob job = new ApplyTransformsJob() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                stashes = stashes
            };

            return job.ScheduleParallel(this.query, 1, stashHandle);
        }
        
        [BurstCompile]
        private struct ApplyTransformsJob : IJobEntityBatchWithIndex {
            public ComponentTypeHandle<Sprite> spriteType;
            
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<TransformStash> stashes;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<Sprite> sprites = batchInChunk.GetNativeArray(this.spriteType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    int index = indexOfFirstEntityInQuery + i;
                    Sprite sprite = sprites[i];
                    
                    TransformStash stash = this.stashes[index];
                    float4x4 rotationTranslationMatrix = new float4x4(stash.rotation, stash.position);
                    float4x4 scaleMatrix = float4x4.Scale(stash.localScale);
                    float4x4 finalMatrix = math.mul(rotationTranslationMatrix, scaleMatrix);
                    sprite.Transform(ref finalMatrix);
                    
                    // Modify
                    sprites[i] = sprite;
                }
            }
        }
    }
}