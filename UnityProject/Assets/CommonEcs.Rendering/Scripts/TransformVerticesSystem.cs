using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

using UnityEngine;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class TransformVerticesSystem : JobSystemBase {
        private EntityQuery query;

        private ComponentTypeHandle<Sprite> spriteType;

        [ReadOnly]
        private ComponentTypeHandle<LocalToWorld> matrixType;

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
            this.spriteType = GetComponentTypeHandle<Sprite>();
            this.matrixType = GetComponentTypeHandle<LocalToWorld>(true);

            Job job = new Job() {
                spriteType = this.spriteType, 
                matrixType = this.matrixType
            };

            return job.ScheduleParallel(this.query, inputDeps);
        }

        [BurstCompile]
        private struct Job : IJobChunk {
            public ComponentTypeHandle<Sprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> matrixType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
                NativeArray<LocalToWorld> matrices = chunk.GetNativeArray(this.matrixType);

                for (int i = 0; i < chunk.Count; ++i) {
                    Sprite sprite = sprites[i];
                    LocalToWorld transform = matrices[i];
                    sprite.Transform(ref transform.Value);

                    sprites[i] = sprite; // Modify the data
                }
            }
        }
    }
}