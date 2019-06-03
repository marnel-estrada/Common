using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(EndPresentationEntityCommandBufferSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UseYAsSortOrderSystem : JobComponentSystem {
        private EntityQuery query;
        private ArchetypeChunkComponentType<Sprite> spriteType;
        private ArchetypeChunkComponentType<Translation> translationType;

        [ReadOnly]
        private ArchetypeChunkComponentType<UseYAsSortOrder> useYType;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Static)
            }, new ComponentType[] {
                typeof(Sprite), typeof(Translation), typeof(UseYAsSortOrder)
            }));
        }

        /// <summary>
        /// Job for ECS sprites
        /// </summary>
        [BurstCompile]
        private struct Job : IJobParallelFor {
            public ArchetypeChunkComponentType<Sprite> spriteType;
            
            [ReadOnly]
            public ArchetypeChunkComponentType<Translation> translationType;
            
            [ReadOnly]
            public ArchetypeChunkComponentType<UseYAsSortOrder> useYType;

            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;
            
            public void Execute(int index) {
                ArchetypeChunk chunk = this.chunks[index];
                Process(ref chunk);                
            }

            private void Process(ref ArchetypeChunk chunk) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
                NativeArray<Translation> translations = chunk.GetNativeArray(this.translationType);
                NativeArray<UseYAsSortOrder> useYArray = chunk.GetNativeArray(this.useYType);

                for (int i = 0; i < chunk.Count; ++i) {
                    UseYAsSortOrder useY = useYArray[i];
                    
                    // We set the order by modifying the z position
                    // The higher the y, the higher it's z. It will be rendered first (painter's algorithm). 
                    Translation translation = translations[i];
    
                    // We use negative of z here because the higher z should be ordered first
                    Sprite sprite = sprites[i];
                    sprite.RenderOrder = -(translation.Value.y + useY.offset);
                    sprites[i] = sprite; // Modify
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.spriteType = GetArchetypeChunkComponentType<Sprite>();
            this.translationType = GetArchetypeChunkComponentType<Translation>();
            this.useYType = GetArchetypeChunkComponentType<UseYAsSortOrder>(true);
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            Job job = new Job() {
                spriteType = this.spriteType,
                translationType = this.translationType,
                useYType = this.useYType,
                chunks = chunks
            };
            
            return job.Schedule(chunks.Length, 64, inputDeps);
        }
    }
}
