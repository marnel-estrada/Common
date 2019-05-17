using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class SignalHandlerComponentSystem<T> : ComponentSystem where T : struct, IComponentData {
        private EntityQuery signalQuery;
        private SignalHandler<T> signalHandler;

        protected override void OnCreateManager() {
            this.signalQuery = GetEntityQuery(typeof(Signal), typeof(T));
            this.signalHandler = new SignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);
        }

        protected abstract void OnDispatch(Entity entity, T component);

        protected override void OnUpdate() {
            this.signalHandler.Update();
        }
    }
}