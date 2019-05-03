using UnityEngine;
using System.Collections;

using Unity.Entities;

namespace Common {
    /// <summary>
    /// Component for duration timing
    /// </summary>
    public struct DurationTimer : IComponentData {

        public float durationTime;
        public float polledTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="durationTime"></param>
        public DurationTimer(float durationTime, float polledTime) {
            this.durationTime = durationTime;
            Assertion.Assert(Comparison.TolerantGreaterThanOrEquals(this.durationTime, 0));

            this.polledTime = polledTime;
        }

        /// <summary>
        /// Constructor with only duration time
        /// </summary>
        /// <param name="durationTime"></param>
        public DurationTimer(float durationTime) : this(durationTime, 0) {
        }

        public bool HasElapsed {
            get {
                return Comparison.TolerantGreaterThanOrEquals(this.polledTime, this.durationTime);
            }
        }

        public float Ratio {
            get {
                float ratio = this.polledTime / this.durationTime;
                return Mathf.Clamp(ratio, 0f, 1f);
            }
        }

        /// <summary>
        /// Resets the timer with the specified duration time
        /// </summary>
        /// <param name="durationTime"></param>
        public void Reset(float durationTime) {
            this.durationTime = durationTime;
            this.polledTime = 0;
        }

        /// <summary>
        /// Resets the timer. Still uses the old duration.
        /// </summary>
        public void Reset() {
            this.polledTime = 0;
        }

        public override string ToString() {
            return "Duration: {0}; PolledTime: {1}".FormatWith(this.durationTime, this.polledTime);
        }

    }
}
