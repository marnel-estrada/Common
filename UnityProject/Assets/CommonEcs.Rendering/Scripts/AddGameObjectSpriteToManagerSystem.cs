using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// This is the same as AddSpriteToManagerSystem but adds the Sprite that are in GameObject world
    /// (Using Transform instead of TransformMatrix)
    /// </summary>
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateAfter(typeof(SpriteManagerInstancesSystem))]
    [UpdateAfter(typeof(AddGameObjectSpriteToLayerSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class AddGameObjectSpriteToManagerSystem : ComponentSystem {
        // Note here that we're not using a common Added component so that each manager knows what 
        // sprite to remove
        public struct Added : ISystemStateComponentData {
            // The entity of the sprite manager to where the sprite is added
            public readonly Entity spriteManagerEntity;
        
            // This is the managerIndex of the sprite so we can determine what index they are if they were somehow
            // removed
            public readonly int managerIndex;

            public Added(Entity spriteManagerEntity, int managerIndex) {
                this.spriteManagerEntity = spriteManagerEntity;
                this.managerIndex = managerIndex;
            }
        }

        private EntityQuery addedQuery;
        private EntityQuery removedQuery;

        private ArchetypeChunkComponentType<Transform> transformType;
        private ArchetypeChunkComponentType<Sprite> spriteType;
        private ArchetypeChunkComponentType<Added> addedType;
        private ArchetypeChunkEntityType entityType;

        private SpriteManagerInstancesSystem spriteManagers;

        protected override void OnCreate() {
            this.addedQuery = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(Transform), typeof(Sprite)
            }, new ComponentType[] {
                typeof(Added), typeof(AddToSpriteLayer)
            }));

            this.removedQuery = GetEntityQuery(this.ConstructQuery(new ComponentType[] {
                typeof(Added)
            }, new ComponentType[] {
                typeof(Transform), typeof(Sprite)
            }));

            this.spriteManagers = this.World.GetOrCreateSystem<SpriteManagerInstancesSystem>();
        }

        protected override void OnUpdate() {
            this.transformType = GetArchetypeChunkComponentType<Transform>();
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.addedType = GetArchetypeChunkComponentType<Added>();
            this.entityType = GetArchetypeChunkEntityType();

            ProcessAdded();
            ProcessRemoved();
        }

        private void ProcessAdded() {
            NativeArray<ArchetypeChunk> chunks = this.addedQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                ProcessAdded(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private NativeArray<Sprite> sprites;
        private ArchetypeChunkComponentObjects<Transform> transforms;
        private NativeArray<Entity> addedEntities;

        private void ProcessAdded(ArchetypeChunk chunk) {
            this.sprites = chunk.GetNativeArray(this.spriteType);
            this.transforms = chunk.GetComponentObjects(this.transformType, this.EntityManager);
            this.addedEntities = chunk.GetNativeArray(this.entityType);

            for (int i = 0; i < chunk.Count; ++i) {
                ProcessAdded(i);
            }
        }

        private void ProcessAdded(int index) {
            Sprite sprite = this.sprites[index];

            if (sprite.spriteManagerEntity == Entity.Null) {
                // The sprite manager entity might not have been set yet
                // For example, when a prefab with SpriteWrapper is instantiated, it probably
                // doesn't have its spriteManagerEntity value set yet.
                // We skip it for now and process them in the next frame.
                return;
            }
            
            Transform transform = this.transforms[index];
            Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(sprite.spriteManagerEntity);
            float4x4 matrix = new float4x4(transform.rotation, transform.position);
            maybeManager.Value.Add(ref sprite, matrix);
            this.sprites[index] = sprite; // Modify the sprite data

            // Add this component so it will no longer be processed by this system
            this.PostUpdateCommands.AddComponent(this.addedEntities[index],
                new Added(sprite.spriteManagerEntity, sprite.managerIndex));
            
            // We add the shared component so that it can be filtered using such shared component
            // in other systems. For example, in SortRenderOrderSystem.
            this.PostUpdateCommands.AddSharedComponent(this.addedEntities[index], maybeManager.Value);
        }

        private void ProcessRemoved() {
            NativeArray<ArchetypeChunk> chunks = this.removedQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                ProcessRemoved(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private NativeArray<Added> addedArray;
        private NativeArray<Entity> removedEntities;

        private void ProcessRemoved(ArchetypeChunk chunk) {
            this.addedArray = chunk.GetNativeArray(this.addedType);
            this.removedEntities = chunk.GetNativeArray(this.entityType);

            for (int i = 0; i < chunk.Count; ++i) {
                ProcessRemoved(i);
            }
        }

        private void ProcessRemoved(int index) {
            Added added = this.addedArray[index];
            Maybe<SpriteManager> maybeManager = this.spriteManagers.Get(added.spriteManagerEntity);
            if (maybeManager.HasValue) {
                maybeManager.Value.Remove(added.managerIndex);
            }

            this.PostUpdateCommands.RemoveComponent<Added>(this.removedEntities[index]);
        }
    }
}
