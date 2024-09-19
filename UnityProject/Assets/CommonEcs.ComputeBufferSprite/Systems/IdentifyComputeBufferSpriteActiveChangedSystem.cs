using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Checks for changes in the active state and marks it as changed
    /// </summary>
    public partial class IdentifyComputeBufferSpriteActiveChangedSystem : SystemBase {
        private EntityQuery activeQuery;
        private EntityQuery inactiveQuery;
        
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);

            this.activeQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<ManagerAdded>()
                .WithAll<Active>()
                .WithNone<ComputeBufferSprite.Changed>()
                .Build(this);
            
            this.inactiveQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<ManagerAdded>()
                .WithNone<Active>()
                .WithNone<ComputeBufferSprite.Changed>()
                .Build(this);
        }

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            ComputeBufferSpriteManager spriteManager = spriteManagers[1];

            TrackDeactivatedJob trackDeactivatedJob = new() {
                managerAddedType = GetComponentTypeHandle<ManagerAdded>(),
                activeType = GetComponentTypeHandle<Active>(),
                activeArray = spriteManager.ActiveArray,
                changedType = GetComponentTypeHandle<ComputeBufferSprite.Changed>(),
                lastSystemVersion = this.LastSystemVersion
            };
            this.Dependency = trackDeactivatedJob.ScheduleParallel(this.inactiveQuery, this.Dependency);
            
            // TODO Schedule tracking of activated sprites
        }
        
        // Checks sprites that were active but turned inactive
        [BurstCompile]
        private struct TrackDeactivatedJob : IJobChunk {
            [ReadOnly]
            public ComponentTypeHandle<ManagerAdded> managerAddedType;

            [ReadOnly]
            public ComponentTypeHandle<Active> activeType;

            [ReadOnly]
            public NativeArray<int> activeArray;
            
            public ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
            
            public uint lastSystemVersion;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                // Check if there were changes before continuing
                if (!chunk.DidChange(ref this.activeType, this.lastSystemVersion)) {
                    return;
                }
                
                NativeArray<ManagerAdded> managerAddedComponents = chunk.GetNativeArray(ref this.managerAddedType);
                
                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    int managerIndex = managerAddedComponents[i].managerIndex;
                    bool activeInArray = this.activeArray[managerIndex] > 0;
                    
                    // We set to changed if activeInArray is true. Note that the query
                    // already queries inactive sprites. So this means that if activeInArray
                    // is true, then it changed to inactive.
                    chunk.SetComponentEnabled(ref this.changedType, i, activeInArray);
                }
            }
        }
    }
}