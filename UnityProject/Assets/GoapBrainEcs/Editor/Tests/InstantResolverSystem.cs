using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(StartConditionResolverSystem))]
    [UpdateBefore(typeof(EndConditionResolverSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class InstantResolverSystem : ComponentSystem {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver), typeof(InstantResolver));
        }

        protected override void OnUpdate() {
            this.Entities.With(this.query).ForEach(delegate(Entity entity, ref ConditionResolver resolver, ref InstantResolver instant) {
                if (resolver.status == ConditionResolverStatus.DONE) {
                    // Already done
                    return;
                }
                
                resolver.result = instant.resolveValue;
                resolver.status = ConditionResolverStatus.DONE;
            });
        }
    }
}