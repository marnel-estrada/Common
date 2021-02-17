using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public interface IFsmActionExecution<T> where T : struct, IComponentData {
        /// <summary>
        /// DotsFsm is passed here so that the action can send events
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <param name="fsm"></param>
        void OnEnter(Entity actionEntity, DotsFsmAction action, ref T actionComponent, ref DotsFsm fsm);
        
        /// <summary>
        /// DotsFsm is passed here so that the action can send events
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        /// <param name="fsm"></param>
        void OnUpdate(Entity actionEntity, DotsFsmAction action, ref T actionComponent, ref DotsFsm fsm);
        
        void OnExit(Entity actionEntity, DotsFsmAction action, ref T actionComponent);
    }
}