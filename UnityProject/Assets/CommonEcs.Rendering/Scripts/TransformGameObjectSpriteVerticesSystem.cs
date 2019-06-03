using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.Jobs;

namespace CommonEcs {
    /// <summary>
    /// This is the same as SpriteManagerTransformSystem but for sprite made in GameObject world
    /// </summary>
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TransformGameObjectSpriteVerticesSystem : JobComponentSystem {
        private EntityQuery query;

        protected override void OnCreateManager() {
            // All entities with Sprite and Transform, but without Static (non Static sprites)
            this.query = GetEntityQuery(typeof(Sprite), typeof(Transform), ComponentType.Exclude<Static>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            TransformAccessArray transforms = this.query.GetTransformAccessArray();
            NativeArray<TransformStash> stashes = new NativeArray<TransformStash>(transforms.length, Allocator.TempJob);
            
            // Job for copying to stashes
            StashTransformsJob stashTransforms = new StashTransformsJob() {
                stashes = stashes
            };
            JobHandle stashHandle = stashTransforms.Schedule(transforms, inputDeps);
            
            // Job for applying to sprites
            ApplyTransforms applyTransforms = new ApplyTransforms() {
                stashes = stashes
            };

            return applyTransforms.Schedule(this.query, stashHandle);
        }

        [BurstCompile]
        private struct ApplyTransforms : IJobForEachWithEntity<Sprite> {
            [DeallocateOnJobCompletion]
            public NativeArray<TransformStash> stashes;

            public void Execute(Entity entity, int index, ref Sprite sprite) {
                TransformStash stash = this.stashes[index];
                float4x4 rotationTranslationMatrix = new float4x4(stash.rotation, stash.position);
                sprite.Transform(ref rotationTranslationMatrix);
            }
        }
    }
}