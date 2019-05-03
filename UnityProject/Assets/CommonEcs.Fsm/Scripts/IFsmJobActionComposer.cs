using Unity.Entities;

namespace Common.Ecs.Fsm {
    public interface IFsmJobActionComposer<T> where T : struct, IFsmJobAction {
        /// <summary>
        /// Composes the job action
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        T Compose(ArchetypeChunk chunk);
    }
}