using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    [UpdateAfter(typeof(AlwaysUpdateVerticesSystem))]
    [UpdateAfter(typeof(UpdateChangedVerticesSystem))]
    [UpdateAfter(typeof(SortRenderOrderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ResetSpriteChangedFlagsSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job();
            return job.Schedule(this, inputDeps);
        }
        
        [BurstCompile]
        private struct Job : IJobForEach<Sprite> {
            public void Execute(ref Sprite sprite) {
                sprite.verticesChanged = false;
                sprite.uvChanged = false;
                sprite.colorChanged = false;
                sprite.renderOrderChanged = false;
            }
        }
    }
}