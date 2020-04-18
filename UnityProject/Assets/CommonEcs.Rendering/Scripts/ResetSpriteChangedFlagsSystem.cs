using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(AlwaysUpdateVerticesSystem))]
    [UpdateAfter(typeof(UpdateChangedVerticesSystem))]
    [UpdateAfter(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ResetSpriteChangedFlagsSystem : SystemBase {
        protected override void OnUpdate() {
            this.Entities.ForEach(delegate(ref Sprite sprite) {
                sprite.verticesChanged = false;
                sprite.uvChanged = false;
                sprite.colorChanged = false;
                sprite.renderOrderChanged = false;
            }).WithBurst().ScheduleParallel();
        }
    }
}