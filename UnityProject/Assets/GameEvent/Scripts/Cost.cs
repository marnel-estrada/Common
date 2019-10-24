using UnityEngine;

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

        /// <summary>
        /// The icon to display the cost with
        /// </summary>
        public virtual string Icon {
            get;
        }

        /// <summary>
        /// The text to display the cost with
        /// </summary>
        public abstract string Text { get; }

        public virtual Color TextColor {
            get;
        }
    }
}