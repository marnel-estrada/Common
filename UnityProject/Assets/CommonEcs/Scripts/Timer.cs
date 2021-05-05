using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// This is like CountdownTimer but can be composed into another struct
    /// </summary>
    public struct Timer {
        public float polledTime;
        public float duration;

        public Timer(float duration) {
            this.polledTime = 0;
            this.duration = duration;
        }

        public void Update(float deltaTime) {
            this.polledTime += deltaTime;
        }

        public void Reset() {
            this.polledTime = 0;
        }
        
        public void Reset(float duration) {
            Reset();
            this.duration = duration;
        }

        public bool HasElapsed {
            get {
                return this.polledTime.TolerantGreaterThanOrEquals(this.duration);
            }
        }

        public float Ratio {
            get {
                float ratio = this.polledTime / this.duration;
                return math.clamp(ratio, 0f, 1f);
            }
        }

        public float Duration {
            get {
                return this.duration;
            }
        }

        /// <summary>
        /// Forces the timer to end
        /// </summary>
        public void End() {
            this.polledTime = this.duration;
        }
    }
}