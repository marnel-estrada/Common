using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(AlwaysUpdateVerticesSystem))]
    [UpdateAfter(typeof(UpdateChangedVerticesSystem))]
    [UpdateAfter(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class ResetSpriteChangedFlagsSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Sprite>()
                .Build(this);
        }

        protected override void OnUpdate() {
            ResetJob resetJob = new() {
                spriteType = GetComponentTypeHandle<Sprite>()
            };
            this.Dependency = resetJob.ScheduleParallel(this.query, this.Dependency);
        }

        private struct ResetJob : IJobChunk {
            public ComponentTypeHandle<Sprite> spriteType;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
                
                DotsAssert.IsFalse(useEnabledMask);
                for (int i = 0; i < chunk.Count; i++) {
                    Sprite sprite = sprites[i];
                    sprite.VerticesChanged = false;
                    sprite.UvChanged = false;
                    sprite.ColorChanged = false;
                    sprite.RenderOrderChanged = false;
                }
            }
        }
    }
}