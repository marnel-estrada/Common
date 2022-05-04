using System;

namespace Common {
    /// <summary>
    /// A handler for a common pattern where an action is only run at some interval and only when it
    /// was marked as dirty.
    /// </summary>
    public class PeriodicDirtyHandler {
        private readonly Action action; // The action to execute
        private readonly CountdownTimer timer;

        private bool isDirty;

        public PeriodicDirtyHandler(float periodDuration, Action action) {
            this.action = action;
            this.timer = new CountdownTimer(periodDuration);
        }

        public void MarkDirty() {
            this.isDirty = true;
        }

        public void Update() {
            this.timer.Update();

            if (!this.timer.HasElapsed()) {
                // Not yet time to execute the action
                return;
            }

            if (this.isDirty) {
                this.action.Invoke();
                this.isDirty = false;
            }
                
            this.timer.Reset();
        }
    }
}