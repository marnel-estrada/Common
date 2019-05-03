using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateAfter(typeof(TransformVerticesSystem))]
    [UpdateAfter(typeof(TransformGameObjectSpriteVerticesSystem))]
    [UpdateAfter(typeof(UseYAsSortOrderSystem))]
    [UpdateAfter(typeof(UseYAsSortOrderGameObjectSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateBefore(typeof(UpdateChangedVerticesSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class IdentifySpriteManagerChangedSystem : ComponentSystem {
        private EntityQuery query;
        private ArchetypeChunkComponentType<Sprite> spriteType;
        
        [ReadOnly]
        private ArchetypeChunkEntityType entityType;

        private readonly List<SpriteManager> managers = new List<SpriteManager>(1);
        private readonly List<int> managerIndices = new List<int>(1);

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(Sprite), typeof(Changed));
        }

        [BurstCompile]
        private struct Job : IJob {
            public ArchetypeChunkComponentType<Sprite> spriteType;

            [ReadOnly]
            public ArchetypeChunkEntityType entityType;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;

            public NativeHashMap<Entity, byte> verticesChangedMap;
            public NativeHashMap<Entity, byte> trianglesChangedMap;
            public NativeHashMap<Entity, byte> uvChangedMap;
            public NativeHashMap<Entity, byte> colorChangedMap;

            public void Execute() {
                for (int i = 0; i < this.chunks.Length; ++i) {
                    ArchetypeChunk chunk = this.chunks[i];
                    Process(ref chunk);
                }
            }

            private void Process(ref ArchetypeChunk chunk) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);

                for (int i = 0; i < chunk.Count; ++i) {
                    Sprite sprite = sprites[i];
                    if (sprite.verticesChanged.Value) {
                        this.verticesChangedMap.TryAdd(sprite.spriteManagerEntity, 0);
                        sprite.verticesChanged.Value = false; // Consume the changed flag
                    }
    
                    if (sprite.renderOrderChanged.Value) {
                        this.trianglesChangedMap.TryAdd(sprite.spriteManagerEntity, 0);
                        sprite.renderOrderChanged.Value = false;
                    }
    
                    if (sprite.uvChanged.Value) {
                        this.uvChangedMap.TryAdd(sprite.spriteManagerEntity, 0);
                        sprite.uvChanged.Value = false;
                    }
    
                    if (sprite.colorChanged.Value) {
                        this.colorChangedMap.TryAdd(sprite.spriteManagerEntity, 0);
                        sprite.colorChanged.Value = false;
                    }
    
                    sprites[i] = sprite; // modify the data
                }
            }
        }

        protected override void OnUpdate() {
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.entityType = GetArchetypeChunkEntityType();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            this.managers.Clear();
            this.managerIndices.Clear();
            this.EntityManager.GetAllUniqueSharedComponentData(this.managers, this.managerIndices);
            
            // We minus 1 because the first entry is always the default entry
            int managerLength = this.managers.Count - 1;
            NativeHashMap<Entity, byte> verticesChangedMap =
                new NativeHashMap<Entity, byte>(managerLength, Allocator.TempJob);
            NativeHashMap<Entity, byte> trianglesChangedMap =
                new NativeHashMap<Entity, byte>(managerLength, Allocator.TempJob);
            NativeHashMap<Entity, byte> uvChangedMap = new NativeHashMap<Entity, byte>(managerLength, Allocator.TempJob);
            NativeHashMap<Entity, byte> colorChangedMap =
                new NativeHashMap<Entity, byte>(managerLength, Allocator.TempJob);

            Job job = new Job() {
                spriteType = this.spriteType,
                entityType = this.entityType,
                chunks = chunks,
                verticesChangedMap = verticesChangedMap,
                trianglesChangedMap = trianglesChangedMap,
                uvChangedMap = uvChangedMap,
                colorChangedMap = colorChangedMap
            };

            job.Schedule().Complete();

            // Process the result
            for (int i = 1; i < this.managers.Count; ++i) {
                SpriteManager manager = this.managers[i];

                if (manager.Owner == Entity.Null) {
                    // The owner for the manager has not been assigned yet
                    // We can skip this
                    continue;
                }

                if (manager.AlwaysUpdateMesh) {
                    // Skip this since it was set as always update
                    // The other flags don't matter
                    continue;
                }

                // Note that we're only checking for existence here
                // We used OR here because the flags might have been already set to true prior to
                // calling this system
                manager.VerticesChanged = manager.VerticesChanged || verticesChangedMap.TryGetValue(manager.Owner, out byte _);
                manager.RenderOrderChanged = manager.RenderOrderChanged || trianglesChangedMap.TryGetValue(manager.Owner, out byte _);
                manager.UvChanged = manager.UvChanged || uvChangedMap.TryGetValue(manager.Owner, out byte _);
                manager.ColorsChanged = manager.ColorsChanged || colorChangedMap.TryGetValue(manager.Owner, out byte _);
            }

            // Dispose
            verticesChangedMap.Dispose();
            trianglesChangedMap.Dispose();
            uvChangedMap.Dispose();
            colorChangedMap.Dispose();
        }

    }
}