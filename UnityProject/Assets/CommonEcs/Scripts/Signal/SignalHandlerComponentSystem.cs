using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class SignalHandlerComponentSystem<T> : ComponentSystem where T : struct, IComponentData {
        private EntityQuery signalQuery;
        private SignalHandler<T> signalHandler;

        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }

        protected override void OnCreate() {
            this.signalQuery = GetEntityQuery(typeof(Signal), typeof(T), ComponentType.Exclude<ProcessedBySystem>());
            this.signalHandler = new SignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);
        }

        protected abstract void OnDispatch(Entity entity, T signalComponent);

        protected override void OnUpdate() {
            this.signalHandler.Update();
            this.PostUpdateCommands.AddComponentForEntityQuery(this.signalQuery, typeof(ProcessedBySystem));
        }
    }
}