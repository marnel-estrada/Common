using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(StartConditionResolverSystem))]
    [UpdateBefore(typeof(EndConditionResolverSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RunningResolverSystem : ComponentSystem {
        private EntityQuery query;

        public const int FRAMES_NEEDED = 5;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver), typeof(RunningResolver));
        }

        protected override void OnUpdate() {
            this.Entities.With(this.query).ForEach(delegate(Entity entity, ref ConditionResolver resolver, ref RunningResolver running) {
                if (resolver.status == ConditionResolverStatus.DONE) {
                    // Already done
                    return;
                }

                ++running.counter;
                if (running.counter >= FRAMES_NEEDED) {
                    resolver.result = true;
                    resolver.status = ConditionResolverStatus.DONE;
                }
            });
        }
    }
}