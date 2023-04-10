using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(AlwaysUpdateVerticesSystem))]
    [UpdateAfter(typeof(UpdateChangedVerticesSystem))]
    [UpdateAfter(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class ResetSpriteChangedFlagsSystem : SystemBase {
        protected override void OnUpdate() {
            this.Entities.WithChangeFilter<Sprite>().ForEach((ref Sprite sprite) => {
                sprite.VerticesChanged = false;
                sprite.UvChanged = false;
                sprite.ColorChanged = false;
                sprite.RenderOrderChanged = false;
            }).WithBurst().ScheduleParallel();
        }
    }
}