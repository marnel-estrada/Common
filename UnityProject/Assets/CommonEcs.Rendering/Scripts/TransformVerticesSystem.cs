using System;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

using UnityEngine;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class TransformVerticesSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            // All entities that has Sprite and LocalToWorld, but no Static
            // Note here that we specifically exclude entities with Transform
            // This system works with pure ECS entities only
            this.query = GetEntityQuery(new EntityQueryDesc() {
                Any = Array.Empty<ComponentType>(),
                None = new ComponentType[] {
                    typeof(Static), typeof(Transform)
                },
                All = new ComponentType[] {
                    typeof(Sprite), typeof(LocalToWorld)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            TransformJob transformJob = new TransformJob() {
                spriteType = GetComponentTypeHandle<Sprite>(), 
                matrixType = GetComponentTypeHandle<LocalToWorld>(true)
            };

            return transformJob.ScheduleParallel(this.query, inputDeps);
        }

        [BurstCompile]
        private struct TransformJob : IJobChunk {
            public ComponentTypeHandle<Sprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> matrixType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<LocalToWorld> matrices = chunk.GetNativeArray(ref this.matrixType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Sprite sprite = sprites[i];
                    LocalToWorld transform = matrices[i];
                    sprite.Transform(ref transform.Value);

                    sprites[i] = sprite; // Modify the data
                }
            }
        }
    }
}