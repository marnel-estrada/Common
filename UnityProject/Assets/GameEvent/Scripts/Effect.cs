using UnityEngine;

namespace GameEvent {
    public abstract class Effect {
        public virtual bool CanApply {
            get {
                return true;
            }
        }
        
        /// <summary>
        /// Applies the effect
        /// </summary>
        public abstract void Apply();

        public virtual string Icon { get; }

        public abstract string Text { get; }
        
        public virtual Color TextColor { get; }
    }
}