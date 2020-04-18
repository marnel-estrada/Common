using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(UpdateComputeBufferSpriteTransformSystem))]
    [UpdateAfter(typeof(CopyTrsFromGameObjectToComputeBufferSpriteSystem))]
    [UpdateBefore(typeof(UpdateChangedSpritesToMasterListSystem))]
    public class UseYAsRenderOrderSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            base.OnCreate();
            this.query = GetEntityQuery(ComponentType.ReadOnly<ComputeBufferSprite>(), 
                typeof(Transform), ComponentType.ReadOnly<UseYAsRenderOrder>(), ComponentType.Exclude<Static>());
        }
        
        // This is used for stashing the position
        private readonly struct YPositionStash {
            // We only need the y position
            public readonly float y;

            public YPositionStash(float y) {
                this.y = y;
            }
        }

        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            TransformAccessArray transforms = this.query.GetTransformAccessArray();
            NativeArray<YPositionStash> stashes = new NativeArray<YPositionStash>(transforms.length, Allocator.TempJob);

            JobHandle handle = inputDeps;

            // Job for copying to stashes
            handle = new StashYPositionJob() {
                stashes = stashes
            }.Schedule(transforms, handle);
            
            handle = new SetRenderOrderJob() {
                spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>(),
                useYType = GetArchetypeChunkComponentType<UseYAsRenderOrder>(),
                stashes = stashes
            }.ScheduleParallel(this.query, handle);

            return handle;
        }

        [BurstCompile]
        private struct StashYPositionJob : IJobParallelForTransform {
            public NativeArray<YPositionStash> stashes;

            public void Execute(int index, TransformAccess transform) {
                this.stashes[index] = new YPositionStash(transform.position.y);
            }
        }
        
        [BurstCompile]
        private struct SetRenderOrderJob : IJobChunk {
            public ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;
            public ArchetypeChunkComponentType<UseYAsRenderOrder> useYType;
            
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<YPositionStash> stashes;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(this.spriteType);
                NativeArray<UseYAsRenderOrder> useYArray = chunk.GetNativeArray(this.useYType);
                
                for (int i = 0; i < sprites.Length; ++i) {
                    int stashIndex = firstEntityIndex + i;
                    YPositionStash stash = this.stashes[stashIndex];
                    ComputeBufferSprite sprite = sprites[i];
                    UseYAsRenderOrder useY = useYArray[i];
                    
                    // We use negative of y here because the higher y should be ordered first
                    sprite.RenderOrder = -(stash.y + useY.offset);
                    
                    // Modify
                    sprites[i] = sprite;
                }
            }
        }
    }
}