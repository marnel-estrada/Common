using Common;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Math.Systems {
    /// <summary>
    /// Updates the Aabb2Component.translation. Gets the position from LocalToWorld.
    /// </summary>
    public partial struct UpdateAabb2System : ISystem {
        private EntityQuery query;

        private ComponentTypeHandle<LocalToWorld> localToWorldType;
        private ComponentTypeHandle<Aabb2Component> aabb2Type;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalToWorld>().WithAll<Aabb2Component>().Build(ref state);

            this.localToWorldType = state.GetComponentTypeHandle<LocalToWorld>();
            this.aabb2Type = state.GetComponentTypeHandle<Aabb2Component>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            this.localToWorldType.Update(ref state);
            this.aabb2Type.Update(ref state);
            
            // Schedule job
            UpdateJob updateJob = new() {
                localToWorldType = this.localToWorldType,
                aabb2Type = this.aabb2Type
            };
            state.Dependency = updateJob.ScheduleParallel(this.query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
        
        [BurstCompile]
        private struct UpdateJob : IJobChunk {
            public ComponentTypeHandle<LocalToWorld> localToWorldType;
            public ComponentTypeHandle<Aabb2Component> aabb2Type;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(ref this.localToWorldType);
                NativeArray<Aabb2Component> aabb2Components = chunk.GetNativeArray(ref this.aabb2Type);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Aabb2Component aabb2 = aabb2Components[i];
                    aabb2.translation = localToWorlds[i].Position.xy;
                    aabb2Components[i] = aabb2; // Modify
                }
            }
        }
    }
}