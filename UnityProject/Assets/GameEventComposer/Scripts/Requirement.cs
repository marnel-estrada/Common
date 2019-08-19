namespace GameEvent {
    /// <summary>
    /// Base class for all requirements. This could be on events or on options.
    /// </summary>
    public abstract class Requirement {
        /// <summary>
        /// Whether or not the requirement is met
        /// </summary>
        /// <returns></returns>
        public abstract bool IsMet();
    }
}