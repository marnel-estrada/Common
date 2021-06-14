using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UseYAsSortOrderSystem : JobComponentSystem {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Static)
            }, new ComponentType[] {
                typeof(Sprite), typeof(Translation), typeof(UseYAsSortOrder)
            }));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                translationType = GetComponentTypeHandle<Translation>(true),
                useYType = GetComponentTypeHandle<UseYAsSortOrder>(true)
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<Sprite> spriteType;
            
            [ReadOnly]
            public ComponentTypeHandle<Translation> translationType;
            
            [ReadOnly]
            public ComponentTypeHandle<UseYAsSortOrder> useYType;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Sprite> sprites = batchInChunk.GetNativeArray(this.spriteType);
                NativeArray<Translation> translations = batchInChunk.GetNativeArray(this.translationType);
                NativeArray<UseYAsSortOrder> useYArray = batchInChunk.GetNativeArray(this.useYType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
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
    }
}
