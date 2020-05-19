using UnityEngine;

namespace Common {
    /// <summary>
    /// A common behaviour that invokes SwarmItem.Kill() after some time
    /// </summary>
    public class TimedRecycle : MonoBehaviour {

        [SerializeField]
        private SwarmItem swarm;

        [SerializeField]
        private float timeBeforeRecycle;

        [SerializeField]
        private string timeReferenceName = "WorldTime"; // May be empty. Uses default if empty

        private CountdownTimer timer;

        public CountdownTimer Timer {
            get => timer;
        }

        private bool ticking;

        void Awake() {
            Assertion.NotNull(this.swarm);
            Assertion.IsTrue(this.timeBeforeRecycle > 0);

            if (string.IsNullOrEmpty(this.timeReferenceName)) {
                // Uses default time reference
                this.timer = new CountdownTimer(this.timeBeforeRecycle);
            } else {
                this.timer = new CountdownTimer(this.timeBeforeRecycle, this.timeReferenceName);
            }

            // client must invoke Begin() to start the ticking
            this.ticking = false;
        }

        /// <summary>
        /// Begins the ticking of time
        /// </summary>
        public void Begin() {
            this.timer.Reset();
            this.ticking = true;
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop() {
            this.timer.Reset();
            this.ticking = false;
        }

        void Update() {
            if(!this.ticking) {
                return;
            }

            this.timer.Update();
            if(this.timer.HasElapsed()) {
                this.swarm.Kill();
                this.ticking = false;
            }
        }

    }
}
