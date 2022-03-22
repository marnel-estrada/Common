using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SignalFramePassedSystem : ComponentSystem {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(Signal), ComponentType.Exclude<SignalFramePassed>());
        }

        protected override void OnUpdate() {
            this.PostUpdateCommands.AddComponentForEntityQuery(this.query, typeof(SignalFramePassed));
        }
    }
}