using Unity.Burst;
using Unity.Burst.Intrinsics;
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
            NativeArray<TransformStash> stashes = new(transforms.length, Allocator.TempJob);
            
            // Job for copying to stashes
            StashTransformsJob stashTransforms = new() {
                stashes = stashes
            };
            JobHandle jobHandle = stashTransforms.Schedule(transforms, inputDeps);
            
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArray(Allocator.TempJob);
            
            // Job for applying to sprites
            ApplyTransformsJob job = new() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                stashes = stashes,
                chunkBaseEntityIndices = chunkBaseEntityIndices
            };

            jobHandle = job.ScheduleParallel(this.query, jobHandle);
            
            // Don't forget to dispose
            jobHandle = chunkBaseEntityIndices.Dispose(jobHandle);
            jobHandle = stashes.Dispose(jobHandle);

            return jobHandle;
        }
        
        [BurstCompile]
        private struct ApplyTransformsJob : IJobChunk {
            public ComponentTypeHandle<Sprite> spriteType;
            
            [ReadOnly]
            public NativeArray<TransformStash> stashes;

            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                EntityIndexAide indexAide = new(ref this.chunkBaseEntityIndices, unfilteredChunkIndex);

                while (enumerator.NextEntityIndex(out int i)) {
                    Sprite sprite = sprites[i];

                    int index = indexAide.NextEntityIndexInQuery();
                    TransformStash stash = this.stashes[index];
                    float4x4 rotationTranslationMatrix = new(stash.rotation, stash.position);
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