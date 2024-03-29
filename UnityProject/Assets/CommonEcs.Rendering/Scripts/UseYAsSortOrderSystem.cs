﻿using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace CommonEcs {
    [UpdateBefore(typeof(TransformVerticesSystem))]
    [UpdateBefore(typeof(SortRenderOrderSystem))]
    [UpdateAfter(typeof(AddSpriteToManagerSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class UseYAsSortOrderSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Static)
            }, new ComponentType[] {
                typeof(Sprite), typeof(LocalToWorld), typeof(UseYAsSortOrder)
            }));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            UseYAsSortOrderJob job = new() {
                spriteType = GetComponentTypeHandle<Sprite>(),
                localToWorldType = GetComponentTypeHandle<LocalToWorld>(true),
                useYType = GetComponentTypeHandle<UseYAsSortOrder>(true)
            };
            
            return job.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct UseYAsSortOrderJob : IJobChunk {
            public ComponentTypeHandle<Sprite> spriteType;

            [ReadOnly]
            public ComponentTypeHandle<LocalToWorld> localToWorldType;
            
            [ReadOnly]
            public ComponentTypeHandle<UseYAsSortOrder> useYType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                NativeArray<LocalToWorld> localToWorldList = chunk.GetNativeArray(ref this.localToWorldType);
                NativeArray<UseYAsSortOrder> useYArray = chunk.GetNativeArray(ref this.useYType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    UseYAsSortOrder useY = useYArray[i];
                    
                    // We set the order by modifying the RenderOrder
                    // The higher the y, the lower its RenderOrder. It will be rendered earlier (painter's algorithm).
                    // Note here that we used the position from LocalToWorld instead of Translation so that entities
                    // that are children will get their actual world position
                    LocalToWorld localToWorld = localToWorldList[i];
    
                    // We use negative of y here because the higher y should be ordered first
                    // Note here that we update RenderOrderDueToPosition instead of RenderOrder
                    // RenderOrder now has higher precedence than RenderOrderDueToPosition.
                    // This is to avoid conflict when there are sprites that are positioned the same but one must
                    // be rendered on top of the other. This is the case for face and head in agents or characters.
                    Sprite sprite = sprites[i];
                    sprite.RenderOrderDueToPosition = -(localToWorld.Position.y + useY.offset);
                    sprites[i] = sprite; // Modify
                }
            }
        }
    }
}
