namespace GameEvent {
    /// <summary>
    /// Generic class for all costs
    /// </summary>
    public abstract class Cost {
        /// <summary>
        /// Returns whether or not the player can afford the cost
        /// </summary>
        public abstract bool CanAfford { get; }

        /// <summary>
        /// Routines to pay the cost
        /// </summary>
        public abstract void Pay();
    }
}