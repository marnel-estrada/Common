using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// The system that resets the Changed component to false
    /// </summary>
    [UpdateInGroup(typeof(ComputeBufferSpriteSystemGroup))]
    public partial struct ResetComputeBufferSpriteChangedSystem : ISystem {
        private EntityQuery query;

        private ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSprite>()
                .WithAll<ComputeBufferSprite.Changed>()
                .Build(ref state);
            state.RequireForUpdate(this.query);

            this.changedType = state.GetComponentTypeHandle<ComputeBufferSprite.Changed>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            this.changedType.Update(ref state);

            ResetChangedJob resetChangedJob = new() {
                changedType = this.changedType
            };
            state.Dependency = resetChangedJob.ScheduleParallel(this.query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
        
        [BurstCompile]
        private struct ResetChangedJob : IJobChunk {
            public ComponentTypeHandle<ComputeBufferSprite.Changed> changedType;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                chunk.SetComponentEnabledForAll(ref this.changedType, false);
            }
        }
    }
}