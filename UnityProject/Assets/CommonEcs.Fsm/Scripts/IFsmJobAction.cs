using Unity.Entities;

namespace Common.Ecs.Fsm {
    public interface IFsmJobAction {
        /// <summary>
        /// Enter routines
        /// </summary>
        /// <param name="index"></param>
        void DoEnter(int index, ref FsmAction action, ref FsmActionUtility utility);

        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="index"></param>
        void DoUpdate(int index, ref FsmAction action, ref FsmActionUtility utility);
    }
}