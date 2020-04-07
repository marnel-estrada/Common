using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(UpdateDrawInstanceArraysSystem))]
    public class PopulateSpriteListSystem : SystemBase {
        private EntityQuery query;
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;

        private ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

        protected override void OnCreate() {
            this.query = GetEntityQuery(ComponentType.ReadOnly<ComputeBufferSprite>(), ComponentType.ReadOnly<ComputeBufferDrawInstance>());
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            // We did it this way so it looks like the old way
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            this.managerQuery.Update();

            JobHandle lastHandle = inputDeps;
            
            this.spriteType = GetArchetypeChunkComponentType<ComputeBufferSprite>();
            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.managerQuery.SharedComponents;
            
            // Note here that we start iteration from 1 because the first value is the default value
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];
                lastHandle = PopulateList(lastHandle, drawInstance);
            }

            return lastHandle;
        }

        private JobHandle PopulateList(JobHandle inputDeps, ComputeBufferDrawInstance drawInstance) {
            this.query.SetSharedComponentFilter(drawInstance);
            drawInstance.ExpandSpritesArray(this.query.CalculateEntityCount());
            
            JobHandle handle = inputDeps;
            handle = new AddToListJob() {
                spriteType = this.spriteType,
                sprites = drawInstance.Sprites
            }.ScheduleParallel(this.query, handle);

            return handle;
        }

        [BurstCompile]
        private struct AddToListJob : IJobChunk {
            [ReadOnly]
            public ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> sprites;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<ComputeBufferSprite> sprites = chunk.GetNativeArray(this.spriteType);
                for (int i = 0; i < sprites.Length; ++i) {
                    this.sprites[firstEntityIndex + i] = sprites[i];
                }
            }
        }
    }
}