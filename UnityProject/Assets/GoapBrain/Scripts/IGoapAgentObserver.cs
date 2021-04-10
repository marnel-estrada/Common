namespace GoapBrain {
    /// <summary>
    /// Common interface for classes that observe GoapAgent
    /// </summary>
    public interface IGoapAgentObserver {

        /// <summary>
        /// Notifies when planning has started
        /// </summary>
        /// <param name="agent"></param>
        void OnPlanningStarted(GoapAgent agent);

        /// <summary>
        /// Notifies when planning has ended
        /// </summary>
        /// <param name="agent"></param>
        void OnPlanningEnded(GoapAgent agent);

        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="agent"></param>
        void Update(GoapAgent agent);

    }
}
