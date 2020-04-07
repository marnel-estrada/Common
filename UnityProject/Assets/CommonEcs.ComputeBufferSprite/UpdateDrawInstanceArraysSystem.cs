using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderComputeBufferSpritesSystem))]
    public class UpdateDrawInstanceArraysSystem : SystemBase {
        private EntityQuery query;
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;

        private ArchetypeChunkComponentType<ComputeBufferSprite> spriteType;

        protected override void OnCreate() {
            this.query = GetEntityQuery(ComponentType.ReadOnly<ComputeBufferSprite>(), ComponentType.ReadOnly<ComputeBufferDrawInstance>());
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.EntityManager.CompleteAllJobs();
            
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
                lastHandle = UpdateArrays(lastHandle, drawInstance);
            }

            return lastHandle;
        }

        private JobHandle UpdateArrays(JobHandle inputDeps, ComputeBufferDrawInstance drawInstance) {
            this.query.SetSharedComponentFilter(drawInstance);
            
            JobHandle handle = inputDeps;

            NativeArray<ComputeBufferSprite> sprites = drawInstance.Sprites;
            drawInstance.Expand(sprites.Length);

            handle = new SetValuesJob() {
                sprites = sprites,
                matrices = drawInstance.Matrices,
                sizePivots = drawInstance.SizePivots,
                uvs = drawInstance.Uvs,
                colors = drawInstance.Colors
            }.Schedule(sprites.Length, 64, handle);

            return handle;
        }
        
        [BurstCompile]
        private struct SetValuesJob : IJobParallelFor {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> sprites;
            
            public NativeArray<float4x4> matrices;
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> colors;
            
            public void Execute(int index) {
                ComputeBufferSprite sprite = this.sprites[index];
                
                this.matrices[index] = sprite.localToWorld;
                this.sizePivots[index] = new float4(sprite.size, sprite.pivot);
                this.uvs[index] = sprite.Uv;
                this.colors[index] = sprite.Color;
            }
        }
    }
}