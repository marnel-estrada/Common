using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// This is like CountdownTimer but can be composed into another struct
    /// </summary>
    public struct Timer {
        public float polledTime;
        private float countdownTime;

        public void Update(float timeDelta) {
            this.polledTime += timeDelta;
        }

        public void Reset() {
            this.polledTime = 0;
        }
        
        public void Reset(float countdownTime) {
            Reset();
            this.countdownTime = countdownTime;
        }

        public bool HasElapsed {
            get {
                return Comparison.TolerantGreaterThanOrEquals(this.polledTime, this.countdownTime);
            }
        }

        public float Ratio {
            get {
                float ratio = this.polledTime / this.countdownTime;
                return math.clamp(ratio, 0f, 1f);
            }
        }

        public float CountdownTime {
            get {
                return this.countdownTime;
            }
        }

        /// <summary>
        /// Forces the timer to end
        /// </summary>
        public void End() {
            this.polledTime = this.countdownTime;
        }
    }
}