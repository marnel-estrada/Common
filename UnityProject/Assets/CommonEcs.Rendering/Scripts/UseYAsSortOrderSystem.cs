using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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
                typeof(Sprite), typeof(Translation), typeof(LocalToWorld), typeof(UseYAsSortOrder)
            }));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                translationType = GetComponentTypeHandle<Translation>(true),
                localToWorldType = GetComponentTypeHandle<LocalToWorld>(true),
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
            public ComponentTypeHandle<LocalToWorld> localToWorldType;
            
            [ReadOnly]
            public ComponentTypeHandle<UseYAsSortOrder> useYType;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<Sprite> sprites = batchInChunk.GetNativeArray(this.spriteType);
                NativeArray<Translation> translations = batchInChunk.GetNativeArray(this.translationType);
                NativeArray<LocalToWorld> localToWorldList = batchInChunk.GetNativeArray(this.localToWorldType);
                NativeArray<UseYAsSortOrder> useYArray = batchInChunk.GetNativeArray(this.useYType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    UseYAsSortOrder useY = useYArray[i];
                    
                    // We set the order by modifying the z position
                    // The higher the y, the higher it's z. It will be rendered first (painter's algorithm).
                    // Note here that we used the transformed position so that entities that are children
                    // will get their actual world position
                    Translation translation = translations[i];
                    LocalToWorld localToWorld = localToWorldList[i];
                    float4 transformed = math.mul(localToWorld.Value, new float4(translation.Value, 1));
    
                    // We use negative of y here because the higher y should be ordered first
                    Sprite sprite = sprites[i];
                    sprite.RenderOrder = -(transformed.y + useY.offset);
                    sprites[i] = sprite; // Modify
                }
            }
        }
    }
}
