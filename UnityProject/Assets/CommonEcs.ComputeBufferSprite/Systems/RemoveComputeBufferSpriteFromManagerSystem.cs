using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// We update this in PresentationSystemGroup since it doesn't use jobs. It will get in the way with
    /// job read/write rules if we update this in simulation group.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class RemoveComputeBufferSpriteFromManagerSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;
        private EntityQuery destroyedSpritesQuery;

        protected override void OnCreate() {
            this.commandBufferSystem = this.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);

            this.destroyedSpritesQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithNone<ComputeBufferSprite>()
                .WithAll<ManagerAdded>()
                .Build(this);
            RequireForUpdate(this.destroyedSpritesQuery);
        }
        
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<ManagerAdded> addedType;

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];
            
            this.entityType = GetEntityTypeHandle();
            this.addedType = GetComponentTypeHandle<ManagerAdded>();
            
            // We can't use Burst compiled jobs here since the Internal class of the sprite
            // manager is a class
            NativeArray<ArchetypeChunk> chunks = this.destroyedSpritesQuery.ToArchetypeChunkArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = this.commandBufferSystem.CreateCommandBuffer();

            for (int i = 0; i < chunks.Length; i++) {
                ProcessChunk(chunks[i], ref spriteManager, ref commandBuffer);
            }

            chunks.Dispose();
        }

        private void ProcessChunk(ArchetypeChunk chunk, ref ComputeBufferSpriteManager spriteManager,
            ref EntityCommandBuffer commandBuffer) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<ManagerAdded> addedComponents = chunk.GetNativeArray(ref this.addedType);

            for (int i = 0; i < chunk.Count; i++) {
                spriteManager.Remove(addedComponents[i].managerIndex);
                
                // We remove Manager added so the entity would be released
                commandBuffer.RemoveComponent<ManagerAdded>(entities[i]);
            }
        }
    }
}