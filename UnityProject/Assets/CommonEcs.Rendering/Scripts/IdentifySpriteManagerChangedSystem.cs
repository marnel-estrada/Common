using System.Collections.Generic;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(TransformVerticesSystem))]
    [UpdateAfter(typeof(UseYAsSortOrderSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateBefore(typeof(UpdateChangedVerticesSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class IdentifySpriteManagerChangedSystem : SystemBase {
        private EntityQuery query;
        private ComponentTypeHandle<Sprite> spriteType;

        private readonly List<SpriteManager> managers = new(1);
        private readonly List<int> managerIndices = new(1);

        protected override void OnCreate() {
            this.query = GetEntityQuery(ComponentType.ReadOnly<Sprite>(), typeof(SpriteManager), 
                ComponentType.Exclude<AlwaysUpdateMesh>());
        }
        
        protected override void OnUpdate() {
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            this.spriteType = GetComponentTypeHandle<Sprite>();
            
            this.managers.Clear();
            this.managerIndices.Clear();
            this.EntityManager.GetAllUniqueSharedComponentsManaged(this.managers, this.managerIndices);
            
            // Note here that we used NativeHashSet and pass it as parallel writer to the job
            // so that we can run the job in parallel. This is the safest way to do it.
            int entityCount = this.query.CalculateEntityCount();
            NativeParallelHashSet<Entity> verticesChangedMap = new(entityCount, WorldUpdateAllocator);
            NativeParallelHashSet<Entity> renderOrderChangedMap = new(entityCount, WorldUpdateAllocator);
            NativeParallelHashSet<Entity> uvChangedMap = new(entityCount, WorldUpdateAllocator);
            NativeParallelHashSet<Entity> colorChangedMap = new(entityCount, WorldUpdateAllocator);

            PopulateUpdatedSpritesJob job = new() {
                spriteType = this.spriteType,
                verticesChangedMap = verticesChangedMap.AsParallelWriter(),
                renderOrderChangedMap = renderOrderChangedMap.AsParallelWriter(),
                uvChangedMap = uvChangedMap.AsParallelWriter(),
                colorChangedMap = colorChangedMap.AsParallelWriter(),
                lastSystemVersion = this.LastSystemVersion
            };
            job.ScheduleParallel(this.query, inputDeps).Complete();

            // Process the result
            for (int i = 1; i < this.managers.Count; ++i) {
                SpriteManager manager = this.managers[i];

                Entity owner = manager.Owner;
                if (owner == Entity.Null) {
                    // The owner for the manager has not been assigned yet
                    // We can skip this
                    continue;
                }

                manager.VerticesChanged = verticesChangedMap.Contains(owner);
                manager.RenderOrderChanged = renderOrderChangedMap.Contains(owner);
                manager.UvChanged = uvChangedMap.Contains(owner);
                manager.ColorsChanged = colorChangedMap.Contains(owner);
            }

            // Dispose
            verticesChangedMap.Dispose();
            renderOrderChangedMap.Dispose();
            uvChangedMap.Dispose();
            colorChangedMap.Dispose();

            return inputDeps;
        }
        
        [BurstCompile]
        private struct PopulateUpdatedSpritesJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<Sprite> spriteType;

            public NativeParallelHashSet<Entity>.ParallelWriter verticesChangedMap;
            public NativeParallelHashSet<Entity>.ParallelWriter renderOrderChangedMap;
            public NativeParallelHashSet<Entity>.ParallelWriter uvChangedMap;
            public NativeParallelHashSet<Entity>.ParallelWriter colorChangedMap;

            public uint lastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                if (!chunk.DidChange(ref this.spriteType, this.lastSystemVersion)) {
                    // This means that the sprites in the chunk have not been queried with write access
                    // There must be no changes at the least
                    return;
                }
                
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Sprite sprite = sprites[i];
                    Entity spriteManagerEntity = sprite.spriteManagerEntity;
                    
                    if (sprite.VerticesChanged) {
                        this.verticesChangedMap.Add(spriteManagerEntity);
                    }

                    if (sprite.RenderOrderChanged) {
                        this.renderOrderChangedMap.Add(spriteManagerEntity);
                    }
    
                    if (sprite.UvChanged) {
                        this.uvChangedMap.Add(spriteManagerEntity);
                    }

                    if (sprite.ColorChanged) {
                        this.colorChangedMap.Add(spriteManagerEntity);
                    }
                }
            }
        }
    }
}