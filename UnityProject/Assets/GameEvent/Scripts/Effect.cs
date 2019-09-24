using UnityEngine;

namespace GameEvent {
    public abstract class Effect {
        /// <summary>
        /// Applies the effect
        /// </summary>
        public abstract void Apply();

        public virtual string Icon { get; }

        public virtual string Text { get; }
        
        public virtual Color TextColor { get; }
    }
}