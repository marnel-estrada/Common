using UnityEngine;

using Common.Time;

namespace Common {
    /**
     * Generic class for implementing timers (specified in seconds)
     */
    public class CountdownTimer {
        private float polledTime;
        private float countdownTime;

        private readonly TimeReference timeReference;

        /**
	     * Constructor with specified TimeReference.
	     */
        public CountdownTimer(float countdownTime, TimeReference timeReference) {
            this.timeReference = timeReference;

            Assertion.IsTrue(countdownTime > 0, "The specified time must be greater than zero.");

            Reset(countdownTime);
        }

        /**
         * Constructor with a specified time reference name.
         */
        public CountdownTimer(float countdownTime, string timeReferenceName) : this(countdownTime,
            TimeReferencePool.GetInstance().Get(timeReferenceName)) {
        }

        /**
         * Constructor that uses a default TimeReference.
         */
        public CountdownTimer(float countdownTime) : this(countdownTime, TimeReference.GetDefaultInstance()) {
        }

        /**
       * Updates the countdown.
       */
        public void Update() {
            this.polledTime += this.timeReference.DeltaTime;
        }

        /**
         * Resets the countdown.
         */
        public void Reset() {
            this.polledTime = 0;
        }

        /**
         * Resets the countdown timer and assigns a new countdown time.
         */
        public void Reset(float countdownTime) {
            Reset();
            this.countdownTime = countdownTime;
        }

        /**
         * Returns whether or not the countdown has elapsed.
         */
        public bool HasElapsed() {
            return this.polledTime.TolerantGreaterThanOrEquals(this.countdownTime);
        }

        /**
         * Returns the ratio of polled time to countdown time.
         */
        public float GetRatio() {
            float ratio = this.polledTime / this.countdownTime;

            return Mathf.Clamp(ratio, 0f, 1f);
        }

        /**
         * Returns the polled time since the countdown started.
         */
        public float GetPolledTime() {
            return this.polledTime;
        }

        /// <summary>
        /// Sets the polled time
        /// </summary>
        /// <param name="polledTime"></param>
        public void SetPolledTime(float polledTime) {
            this.polledTime = polledTime;
        }

        /**
         * Forces the timer to end.
         */
        public void EndTimer() {
            this.polledTime = this.countdownTime;
        }

        /**
         * Adjusts the countdownTime
         */
        public void SetCountdownTime(float newTime) {
            this.countdownTime = newTime;
        }

        /**
         * Gets the countdownTime
         */
        public float GetCountdownTime() {
            return this.countdownTime;
        }

        /**
         * Gets the countdown time as a string.
         */
        public string GetCountdownTimeString() {
            float timeRemaining = this.countdownTime - this.polledTime;
            int minutes = (int) (timeRemaining / 60.0f);
            int seconds = ((int) timeRemaining) % 60;

            return $"{minutes}:{seconds:d2}";
        }

        public TimeReference TimeReference {
            get {
                return this.timeReference;
            }
        }
    }
}

