using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public interface IFsmActionExecution<T> where T : struct, IComponentData {
        void OnEnter(Entity actionEntity, DotsFsmAction action, ref T actionComponent);
        
        void OnUpdate(Entity actionEntity, DotsFsmAction action, ref T actionComponent);
        
        void OnExit(Entity actionEntity, DotsFsmAction action, ref T actionComponent);
    }
}