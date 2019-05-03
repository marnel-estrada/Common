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
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TransformGameObjectSpriteVerticesSystem : ComponentSystem {
        private EntityQuery query;
    
        protected override void OnCreateManager() {
            // All entities with Sprite and Transform, but without Static (non Static sprites)
            this.query = GetEntityQuery(typeof(Sprite), typeof(Transform), ComponentType.Exclude<Static>());
        }
    
        protected override void OnUpdate() {
            NativeArray<Sprite> sprites = this.query.ToComponentDataArray<Sprite>(Allocator.TempJob, 
                out JobHandle spritesJob);
            
            TransformJob transformJob = new TransformJob() {
                sprites = sprites
            };
            transformJob.Schedule(this.query.GetTransformAccessArray(), spritesJob).Complete();
            
            this.query.CopyFromComponentDataArray(sprites);
            
            sprites.Dispose();
        }
        
        [BurstCompile]
        private struct TransformJob : IJobParallelForTransform {
            public NativeArray<Sprite> sprites;
            
            public void Execute(int index, TransformAccess transform) {
                Sprite sprite = this.sprites[index];
                float4x4 rotationTranslationMatrix = new float4x4(transform.rotation, transform.position);
                float4x4 scaleMatrix = float4x4.Scale(transform.localScale);
                float4x4 matrix = math.mul(rotationTranslationMatrix, scaleMatrix);
                sprite.Transform(ref matrix);
        
                this.sprites[index] = sprite; // Modify the data
            }
        }
    }
}