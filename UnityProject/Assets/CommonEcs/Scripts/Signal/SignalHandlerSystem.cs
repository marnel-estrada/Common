using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// This is used for signal handler systems that responds to the same signal parameter
    /// We used recursive generic type so that there's a unique ProcessedBySystem for each
    /// system that derives from this class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    [UpdateBefore(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class SignalHandlerSystem<T, S> : ComponentSystem where T : struct, IComponentData
        where S : SignalHandlerSystem<T, S> {
        private SignalHandler<T> signalHandler;
        private EntityQuery signalQuery;

        protected override void OnCreate() {
            this.signalQuery = GetEntityQuery(typeof(Signal), typeof(T), ComponentType.Exclude<ProcessedBySystem>());
            this.signalHandler = new SignalHandler<T>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);
        }

        protected abstract void OnDispatch(Entity entity, T signalComponent);

        protected override void OnUpdate() {
            this.signalHandler.Update();
            this.PostUpdateCommands.AddComponent(this.signalQuery, typeof(ProcessedBySystem));
        }

        // Tag that identifies a signal entity that it has been already processed
        public struct ProcessedBySystem : IComponentData {
        }
    }
}