using Unity.Entities;

namespace CommonEcs.DotsFsm {
    public interface IFsmActionExecution<T> where T : struct, IFsmActionComponent {
        /// <summary>
        /// DotsFsm is passed here so that the action can send events
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        void OnEnter(Entity actionEntity, ref DotsFsmAction action, ref T actionComponent);
        
        /// <summary>
        /// DotsFsm is passed here so that the action can send events
        /// </summary>
        /// <param name="actionEntity"></param>
        /// <param name="action"></param>
        /// <param name="actionComponent"></param>
        void OnUpdate(Entity actionEntity, ref DotsFsmAction action, ref T actionComponent);
        
        void OnExit(Entity actionEntity, DotsFsmAction action, ref T actionComponent);
    }
}