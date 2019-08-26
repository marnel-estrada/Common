using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(DestroySignalsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class SignalHandlerWithCustomProcessedTypeSystem<ParameterType, ProcessedType> : ComponentSystem
        where ParameterType : struct, IComponentData where ProcessedType : struct, IComponentData {
        private EntityQuery signalQuery;
        private SignalHandler<ParameterType> signalHandler;

        protected override void OnCreate() {
            this.signalQuery = GetEntityQuery(typeof(Signal), typeof(ParameterType), ComponentType.Exclude<ProcessedType>());
            this.signalHandler = new SignalHandler<ParameterType>(this, this.signalQuery);
            this.signalHandler.AddListener(OnDispatch);
        }

        protected abstract void OnDispatch(Entity entity, ParameterType signalComponent);

        protected override void OnUpdate() {
            this.signalHandler.Update();
            this.PostUpdateCommands.AddComponent(this.signalQuery, typeof(ProcessedType));
        }
    }
}