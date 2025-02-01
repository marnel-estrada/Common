using Unity.Entities;

namespace CommonEcs.UtilityBrain {
    /// <summary>
    /// An interface that contains the methods for a consideration system to run. 
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <typeparam name="TProcess"></typeparam>
    public interface IConsiderationSystemStrategy<TComponent, out TProcess> 
        where TComponent : unmanaged, IConsiderationComponent 
        where TProcess : unmanaged, IConsiderationProcess<TComponent> {
        /// <summary>
        /// OnCreate routines. The strategy might want to prepare something.
        /// </summary>
        /// <param name="state"></param>
        void OnCreate(ref SystemState state);

        /// <summary>
        /// Whether or not the system could execute. The system could be waiting for something before
        /// it can execute.
        /// </summary>
        bool CanExecute(ref SystemState state);

        bool ShouldScheduleParallel { get; }

        /// <summary>
        /// Creates the processor struct.
        /// </summary>
        /// <returns></returns>
        TProcess CreateProcess(ref SystemState state);

        /// <summary>
        /// OnDestroy routines. The strategy might want to dispose something.
        /// </summary>
        /// <param name="state"></param>
        void OnDestroy(ref SystemState state);
    }
}