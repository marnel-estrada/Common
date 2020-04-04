using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UpdateDrawInstanceArraysSystem : SystemBase {
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
                lastHandle = UpdateArrays(lastHandle, drawInstance);
            }

            return lastHandle;
        }

        private JobHandle UpdateArrays(JobHandle inputDeps, ComputeBufferDrawInstance drawInstance) {
            this.query.SetSharedComponentFilter(drawInstance);
            
            JobHandle handle = inputDeps;

            int count = this.query.CalculateEntityCount();
            drawInstance.Expand(count);
            
            handle = new SetValuesJob() {
                sprites = drawInstance.Sprites,
                sizePivots = drawInstance.SizePivots,
                uvs = drawInstance.Uvs,
                colors = drawInstance.Colors,
                transforms = drawInstance.Transforms,
                rotations = drawInstance.Rotations
            }.Schedule(count, 64, handle);

            return handle;
        }
        
        private struct SetValuesJob : IJobParallelFor {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeList<ComputeBufferSprite> sprites;
            
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> colors;
            
            public NativeArray<float4> transforms;
            public NativeArray<float> rotations;
            
            public void Execute(int index) {
                ComputeBufferSprite sprite = this.sprites[index];
                
                this.sizePivots[index] = new float4(sprite.size, sprite.pivot);
                this.uvs[index] = sprite.Uv;
                this.colors[index] = sprite.Color;
                this.transforms[index] = sprite.transform;
                this.rotations[index] = sprite.rotation;
            }
        }
    }
}