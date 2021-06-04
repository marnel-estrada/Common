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
    public class IdentifySpriteManagerChangedSystem : SystemBase {
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
            
            // Populate index map
            // This is a mapping of the SpriteManager entity to its index in the NativeArray that will 
            // represent if something changed to sprites belonging to a draw instance.
            // This used to be implemented as a NativeHashMap. We changed it to NativeArray so we can
            // run it in parallel
            NativeHashMap<Entity, int> managerToIndexMap = new NativeHashMap<Entity, int>(4, Allocator.TempJob);
            for (int i = 1; i < this.managers.Count; ++i) {
                managerToIndexMap.TryAdd(this.managers[i].Owner, i - 1);
            }
            
            // We minus 1 because the first entry is always the default entry
            int managerLength = this.managers.Count - 1;
            NativeArray<bool> verticesChangedMap = new NativeArray<bool>(managerLength, Allocator.TempJob);
            NativeArray<bool> trianglesChangedMap = new NativeArray<bool>(managerLength, Allocator.TempJob);
            NativeArray<bool> uvChangedMap = new NativeArray<bool>(managerLength, Allocator.TempJob);
            NativeArray<bool> colorChangedMap = new NativeArray<bool>(managerLength, Allocator.TempJob);

            Job job = new Job() {
                spriteType = this.spriteType,
                managerToIndexMap = managerToIndexMap,
                verticesChangedMap = verticesChangedMap,
                trianglesChangedMap = trianglesChangedMap,
                uvChangedMap = uvChangedMap,
                colorChangedMap = colorChangedMap,
                lastSystemVersion = this.LastSystemVersion
            };

            job.ScheduleParallel(this.query, inputDeps).Complete();

            // Process the result
            for (int i = 1; i < this.managers.Count; ++i) {
                SpriteManager manager = this.managers[i];

                if (manager.Owner == Entity.Null) {
                    // The owner for the manager has not been assigned yet
                    // We can skip this
                    continue;
                }

                int changedIndex = managerToIndexMap[manager.Owner];

                // Note that we're only checking for existence here
                // We used OR here because the flags might have been already set to true prior to
                // calling this system
                manager.VerticesChanged = manager.VerticesChanged || verticesChangedMap[changedIndex];
                manager.RenderOrderChanged = manager.RenderOrderChanged || trianglesChangedMap[changedIndex];
                manager.UvChanged = manager.UvChanged || uvChangedMap[changedIndex];
                manager.ColorsChanged = manager.ColorsChanged || colorChangedMap[changedIndex];
            }

            // Dispose
            managerToIndexMap.Dispose();
            verticesChangedMap.Dispose();
            trianglesChangedMap.Dispose();
            uvChangedMap.Dispose();
            colorChangedMap.Dispose();

            return inputDeps;
        }
        
        [BurstCompile]
        private struct Job : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<Sprite> spriteType;
            
            [ReadOnly]
            public NativeHashMap<Entity, int> managerToIndexMap;

            [NativeDisableParallelForRestriction]
            public NativeArray<bool> verticesChangedMap;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<bool> trianglesChangedMap;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<bool> uvChangedMap;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<bool> colorChangedMap;

            public uint lastSystemVersion;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                if (!chunk.DidChange(this.spriteType, this.lastSystemVersion)) {
                    // This means that the sprites in the chunk have not been queried with write access
                    // There must be no changes at the least
                    return;
                }
                
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);

                for (int i = 0; i < chunk.Count; ++i) {
                    Sprite sprite = sprites[i];
                    int changedIndex = this.managerToIndexMap[sprite.spriteManagerEntity];

                    this.verticesChangedMap[changedIndex] =
                        this.verticesChangedMap[changedIndex] || sprite.verticesChanged.Value; 
    
                    this.trianglesChangedMap[changedIndex] = this.trianglesChangedMap[changedIndex] || sprite.renderOrderChanged.Value;
    
                    this.uvChangedMap[changedIndex] = this.uvChangedMap[changedIndex] || sprite.uvChanged.Value;
    
                    this.colorChangedMap[changedIndex] = this.colorChangedMap[changedIndex] || sprite.colorChanged.Value;
                }
            }
        }
    }
}