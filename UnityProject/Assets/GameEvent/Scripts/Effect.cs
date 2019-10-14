using UnityEngine;

namespace GameEvent {
    public abstract class Effect {
        /// <summary>
        /// Applies the effect
        /// </summary>
        public abstract void Apply();

        public virtual string Icon { get; }

        public abstract string Text { get; }
        
        public virtual Color TextColor { get; }

        // There may be effects that can't execute because some precondition is not met
        public virtual bool CanBeApplied {
            get {
                return true;
            }
        }
    }
}