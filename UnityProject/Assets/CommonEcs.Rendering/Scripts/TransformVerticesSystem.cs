using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TransformVerticesSystem : JobComponentSystem {
        private EntityQuery query;

        private ArchetypeChunkComponentType<Sprite> spriteType;

        [ReadOnly]
        private ArchetypeChunkComponentType<LocalToWorld> matrixType;

        [BurstCompile]
        private struct Job : IJobParallelFor {
            public ArchetypeChunkComponentType<Sprite> spriteType;

            [ReadOnly]
            public ArchetypeChunkComponentType<LocalToWorld> matrixType;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;

            public void Execute(int index) {
                ArchetypeChunk chunk = this.chunks[index];
                Process(ref chunk);
            }

            private void Process(ref ArchetypeChunk chunk) {
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

        protected override void OnCreateManager() {
            // All entities that has Sprite and LocalToWorld, but no Static
            this.query = GetEntityQuery(new EntityQueryDesc() {
                Any = Array.Empty<ComponentType>(),
                None = new ComponentType[] {
                    typeof(Static)
                },
                All = new ComponentType[] {
                    typeof(Sprite), typeof(LocalToWorld)
                }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.matrixType = GetArchetypeChunkComponentType<LocalToWorld>(true);

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);

            Job job = new Job() {
                spriteType = this.spriteType,
                matrixType = this.matrixType,
                chunks = chunks
            };

            return job.Schedule(chunks.Length, 64, inputDeps);
        }
    }
}