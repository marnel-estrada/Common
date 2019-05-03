using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateBefore(typeof(TransformGameObjectSpriteVerticesSystem))]
    [UpdateBefore(typeof(EndPresentationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UseYAsSortOrderGameObjectSystem : ComponentSystem {
        private EntityQuery query;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(
                typeof(Sprite), 
                typeof(Transform),
                ComponentType.ReadOnly<UseYAsSortOrder>(),
                ComponentType.Exclude<Static>()
            );
        }

        protected override void OnUpdate() {
            NativeArray<Sprite> sprites = this.query.ToComponentDataArray<Sprite>(Allocator.TempJob, 
                out JobHandle spritesHandle);
            NativeArray<UseYAsSortOrder> sortOrders = this.query.ToComponentDataArray<UseYAsSortOrder>(Allocator.TempJob, 
                out JobHandle sortOrdersHandle);
            JobHandle combinedHandle = JobHandle.CombineDependencies(spritesHandle, sortOrdersHandle);
            
            TransformJob job = new TransformJob() {
                sprites = sprites,
                sortOrders = sortOrders
            };
            job.Schedule(this.query.GetTransformAccessArray(), combinedHandle).Complete();
            
            this.query.CopyFromComponentDataArray(sprites);
            
            sprites.Dispose();
            sortOrders.Dispose();
        }
        
        [BurstCompile]
        private struct TransformJob : IJobParallelForTransform {
            public NativeArray<Sprite> sprites;

            [ReadOnly]
            public NativeArray<UseYAsSortOrder> sortOrders;
            
            public void Execute(int index, TransformAccess transform) {
                Vector3 position = transform.position;
                
                // We use negative of z here because the higher z should be ordered first
                Sprite sprite = this.sprites[index];
                sprite.RenderOrder = -(position.y + this.sortOrders[index].offset);
                this.sprites[index] = sprite; // Modify
            }
        }
    }
}
