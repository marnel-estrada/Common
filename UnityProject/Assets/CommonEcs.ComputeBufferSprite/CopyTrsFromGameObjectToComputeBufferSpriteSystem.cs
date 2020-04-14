using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    public class CopyTrsFromGameObjectToComputeBufferSpriteSystem : SystemBase {
        private EntityQuery query;
        private ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

        protected override void OnCreate() {
            // All entities with Sprite and Transform, but without Static (non Static sprites)
            this.query = GetEntityQuery(typeof(ComputeBufferSprite), typeof(Transform), ComponentType.Exclude<Static>());
        }
        
        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            TransformAccessArray transforms = this.query.GetTransformAccessArray();
            NativeArray<TransformForSpriteStash> stashes = new NativeArray<TransformForSpriteStash>(transforms.length, Allocator.TempJob);
            
            // Job for copying to stashes
            StashSpriteTransformsJob stashTransforms = new StashSpriteTransformsJob() {
                stashes = stashes
            };
            JobHandle handle = stashTransforms.Schedule(transforms, inputDeps);

            this.spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>();
            handle = new SetStashToSpriteJob() {
                spriteType = this.spriteType, stashes = stashes
            }.ScheduleParallel(this.query, handle);

            return handle;
        }

        private struct TransformForSpriteStash {
            public float2 position;
            public float2 scale;
            public float rotation;
        }
        
        private struct StashSpriteTransformsJob : IJobParallelForTransform {
            public NativeArray<TransformForSpriteStash> stashes;
            
            public void Execute(int index, TransformAccess transform) {
                this.stashes[index] = new TransformForSpriteStash() {
                    position = new float2(transform.position.x, transform.position.y),
                    scale = new float2(transform.localScale.x, transform.localScale.y),
                    rotation = math.radians(transform.rotation.eulerAngles.z)
                };
            }
        }
        
        private struct SetStashToSpriteJob : IJobChunk {
            public ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<TransformForSpriteStash> stashes;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(this.spriteType);
                for (int i = 0; i < sprites.Length; ++i) {
                    int stashIndex = firstEntityIndex + i;
                    TransformForSpriteStash stash = this.stashes[stashIndex];
                    ComputeBufferSprite sprite = sprites[i];
                    sprite.SetTransform(stash.position, stash.scale);
                    sprite.rotation = stash.rotation;
                    
                    // Modify
                    sprites[i] = sprite;
                }
            }
        }
    }
}