using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CommonEcs {
    /// <summary>
    /// We update this in PresentationSystemGroup since it doesn't use jobs. It will get in the way with
    /// job read/write rules if we update this in simulation group.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class AddComputeBufferSpriteToManagerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;
        private EntityQuery spritesQuery;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
            
            this.spritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<ComputeBufferSpriteLayer>()
                .WithAll<UvIndex>()
                .WithNone<ManagerAdded>()
                .Build(this);
            RequireForUpdate(this.spritesQuery);
        }

        private EntityTypeHandle entityType;
        private SharedComponentTypeHandle<ComputeBufferSpriteLayer> layerType;
        private ComponentTypeHandle<ComputeBufferSprite> spriteType;
        private ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
        private BufferTypeHandle<UvIndex> uvIndexType;
        private ComponentTypeHandle<LocalTransform> localTransformType;
        private ComponentTypeHandle<LocalToWorld> worldTransformType;

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];

            this.entityType = GetEntityTypeHandle();
            this.layerType = GetSharedComponentTypeHandle<ComputeBufferSpriteLayer>();
            this.spriteType = GetComponentTypeHandle<ComputeBufferSprite>();
            this.changedType = GetComponentTypeHandle<ComputeBufferSprite.Changed>();
            this.uvIndexType = GetBufferTypeHandle<UvIndex>(true);
            this.localTransformType = GetComponentTypeHandle<LocalTransform>(true);
            this.worldTransformType = GetComponentTypeHandle<LocalToWorld>(true);

            // We can't use Burst compiled jobs here since the Internal class of the sprite
            // manager is a class
            NativeArray<ArchetypeChunk> chunks = this.spritesQuery.ToArchetypeChunkArray(WorldUpdateAllocator);
            EntityCommandBuffer commandBuffer = new(WorldUpdateAllocator);

            for (int i = 0; i < chunks.Length; i++) {
                ProcessChunk(chunks[i], ref spriteManager, ref commandBuffer);
            }
            
            chunks.Dispose();
            
            commandBuffer.Playback(this.EntityManager);
        }

        private void ProcessChunk(ArchetypeChunk chunk, ref ComputeBufferSpriteManager spriteManager,
            ref EntityCommandBuffer commandBuffer) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(ref this.spriteType);
            BufferAccessor<UvIndex> uvIndicesAccessor = chunk.GetBufferAccessor(ref this.uvIndexType);
            NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref this.localTransformType);
            NativeArray<LocalToWorld> worldTransforms = chunk.GetNativeArray(ref this.worldTransformType);
            ComputeBufferSpriteLayer layer = chunk.GetSharedComponent(this.layerType);

            for (int i = 0; i < chunk.Count; i++) {
                ComputeBufferSprite sprite = sprites[i];
                DynamicBuffer<UvIndex> uvIndexBuffer = uvIndicesAccessor[i];
                LocalTransform localTransform = localTransforms[i];
                LocalToWorld worldTransform = worldTransforms[i];

                float3 position = worldTransform.Position;
                position.z += ComputeBufferSpriteUtils.ComputeZPos(layer.value, position.y);
                int managerIndex = spriteManager.Add(ref sprite, position, worldTransform.Rotation, localTransform.Scale);
                
                // Set the uvIndex
                for (int uvIndex = 0; uvIndex < uvIndexBuffer.Length; uvIndex++) {
                    spriteManager.SetUvIndex(managerIndex, uvIndex, uvIndexBuffer[uvIndex].value);
                }
                
                // Add this component so it will no longer be processed by this system
                commandBuffer.AddComponent(entities[i], new ManagerAdded(managerIndex));
            }
            
            chunk.SetComponentEnabledForAll(ref this.changedType, true);
        }
    }
}