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
        private SharedComponentQuery<ComputeBufferDrawInstance> managerQuery;

        protected override void OnCreate() {
            this.managerQuery = new SharedComponentQuery<ComputeBufferDrawInstance>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.EntityManager.CompleteAllJobs();
            
            // We did it this way so it looks like the old way
            this.Dependency = OnUpdate(this.Dependency);
        }

        private JobHandle OnUpdate(JobHandle inputDeps) {
            this.managerQuery.Update();

            IReadOnlyList<ComputeBufferDrawInstance> drawInstances = this.managerQuery.SharedComponents;
            JobHandle lastHandle = inputDeps;
            
            // Note here that we start iteration from 1 because the first value is the default value
            for (int i = 1; i < drawInstances.Count; ++i) {
                ComputeBufferDrawInstance drawInstance = drawInstances[i];

                // Sort only if render order changed
                if (!drawInstance.RenderOrderChanged) {
                    continue;
                }
                
                NativeArray<ComputeBufferSprite> masterList = drawInstance.SpritesMasterList;
                NativeArray<SortEntry> sortEntries = new NativeArray<SortEntry>(masterList.Length, Allocator.TempJob);
                
                // Populate sortEntries
                lastHandle = new PopulateJob() {
                    spriteMasterList = masterList,
                    sortEntries = sortEntries
                }.Schedule(masterList.Length, 64, lastHandle);
                
                // Sort (SortJob is fast!)
                lastHandle = sortEntries.SortJob(lastHandle);
                
                // Update arrays
                // sortEntries will be deallocated on this job
                lastHandle = new SetValuesJob() {
                    transforms = drawInstance.Transforms,
                    rotations = drawInstance.Rotations,
                    spriteMasterList = masterList,
                    sortEntries = sortEntries,
                    sizePivots = drawInstance.SizePivots,
                    uvs = drawInstance.Uvs,
                    colors = drawInstance.Colors
                }.Schedule(masterList.Length, 64, lastHandle);
            }

            return lastHandle;
        }

        [BurstCompile]
        private struct PopulateJob : IJobParallelFor {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> spriteMasterList;

            [NativeDisableParallelForRestriction]
            public NativeArray<SortEntry> sortEntries;

            public void Execute(int index) {
                ComputeBufferSprite sprite = this.spriteMasterList[index];
                this.sortEntries[index] = new SortEntry(index, sprite.layerOrder, sprite.renderOrder); 
            }
        }
        
        [BurstCompile]
        private struct SetValuesJob : IJobParallelFor {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ComputeBufferSprite> spriteMasterList;

            [ReadOnly]
            [NativeDisableParallelForRestriction]
            [DeallocateOnJobCompletion]
            public NativeArray<SortEntry> sortEntries;
            
            public NativeArray<float4> transforms;
            public NativeArray<float> rotations;
            public NativeArray<float4> sizePivots;
            public NativeArray<float4> uvs;
            public NativeArray<float4> colors;
            
            public void Execute(int index) {
                int spriteIndex = this.sortEntries[index].index;
                ComputeBufferSprite sprite = this.spriteMasterList[spriteIndex];
                
                this.transforms[index] = sprite.transform;
                this.rotations[index] = sprite.rotation;
                this.sizePivots[index] = new float4(sprite.size, sprite.pivot);
                this.uvs[index] = sprite.Uv;
                this.colors[index] = sprite.Color;
            }
        }
    }
}