using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(TransformVerticesSystem))]
    [UpdateAfter(typeof(TransformGameObjectSpriteVerticesSystem))]
    [UpdateAfter(typeof(UseYAsSortOrderSystem))]
    [UpdateAfter(typeof(UseYAsSortOrderGameObjectSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateBefore(typeof(UpdateChangedVerticesSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class IdentifySpriteManagerChangedSystem : SystemBase {
        private EntityQuery query;
        private ComponentTypeHandle<Sprite> spriteType;

        private readonly List<SpriteManager> managers = new List<SpriteManager>(1);
        private readonly List<int> managerIndices = new List<int>(1);

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
            this.EntityManager.GetAllUniqueSharedComponentData(this.managers, this.managerIndices);
            
            // Note here that we used NativeHashSet and pass it as parallel writer to the job
            // so that we can run the job in parallel. This is the safest way to do it.
            int entityCount = this.query.CalculateEntityCount();
            NativeParallelHashSet<Entity> verticesChangedMap = new NativeParallelHashSet<Entity>(entityCount, Allocator.TempJob);
            NativeParallelHashSet<Entity> trianglesChangedMap = new NativeParallelHashSet<Entity>(entityCount, Allocator.TempJob);
            NativeParallelHashSet<Entity> uvChangedMap = new NativeParallelHashSet<Entity>(entityCount, Allocator.TempJob);
            NativeParallelHashSet<Entity> colorChangedMap = new NativeParallelHashSet<Entity>(entityCount, Allocator.TempJob);

            Job job = new Job() {
                spriteType = this.spriteType,
                verticesChangedMap = verticesChangedMap.AsParallelWriter(),
                trianglesChangedMap = trianglesChangedMap.AsParallelWriter(),
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

                // Note that we're only checking for existence here
                // We used OR here because the flags might have been already set to true prior to
                // calling this system
                manager.VerticesChanged = verticesChangedMap.Contains(owner);
                manager.RenderOrderChanged = trianglesChangedMap.Contains(owner);
                manager.UvChanged = uvChangedMap.Contains(owner);
                manager.ColorsChanged = colorChangedMap.Contains(owner);
            }

            // Dispose
            verticesChangedMap.Dispose();
            trianglesChangedMap.Dispose();
            uvChangedMap.Dispose();
            colorChangedMap.Dispose();

            return inputDeps;
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<Sprite> spriteType;

            public NativeParallelHashSet<Entity>.ParallelWriter verticesChangedMap;
            public NativeParallelHashSet<Entity>.ParallelWriter trianglesChangedMap;
            public NativeParallelHashSet<Entity>.ParallelWriter uvChangedMap;
            public NativeParallelHashSet<Entity>.ParallelWriter colorChangedMap;

            public uint lastSystemVersion;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                if (!batchInChunk.DidChange(this.spriteType, this.lastSystemVersion)) {
                    // This means that the sprites in the chunk have not been queried with write access
                    // There must be no changes at the least
                    return;
                }
                
                NativeArray<Sprite> sprites = batchInChunk.GetNativeArray(this.spriteType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    Sprite sprite = sprites[i];
                    Entity spriteManagerEntity = sprite.spriteManagerEntity;
                    
                    if (sprite.VerticesChanged) {
                        this.verticesChangedMap.Add(spriteManagerEntity);
                    }

                    if (sprite.RenderOrderChanged) {
                        this.trianglesChangedMap.Add(spriteManagerEntity);
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