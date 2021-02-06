using Unity.Entities;

namespace CommonEcs {
    public interface ISignalProcessor<T> where T : struct, IComponentData {
        void Execute(Entity signalEntity, T parameter);
    }
}