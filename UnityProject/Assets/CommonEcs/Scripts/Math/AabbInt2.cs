using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// An axis aligned bounding box that uses int2 coordinates
    /// This may be used as bounding box for tile based coordinates
    /// </summary>
    public struct AabbInt2 {
        private int2 min;
        private int2 max;

        public static AabbInt2 EmptyAabb() {
            // It's empty if we specify a higher min than max
            return new AabbInt2(new int2(int.MaxValue, int.MaxValue), new int2(int.MinValue, int.MinValue));
        }

        public AabbInt2(int2 min, int2 max) {
            this.min = min;
            this.max = max;
        }

        public void Clear() {
            this.min = new int2(int.MaxValue, int.MaxValue);
            this.max = new int2(int.MinValue, int.MinValue);
        }

        public void Add(int2 point) {
            // Expand min
            if (point.x < this.min.x) {
                this.min.x = point.x;
            }

            if (point.y < this.min.y) {
                this.min.y = point.y;
            }

            // Expand max
            if (point.x > this.max.x) {
                this.max.x = point.x;
            }

            if (point.y > this.max.y) {
                this.max.y = point.y;
            }
        }

        public int2 Min {
            get {
                return this.min;
            }
        }

        public int2 Max {
            get {
                return this.max;
            }
        }

        public int Width {
            get {
                return this.max.x - this.min.x;
            }
        }

        public int Height {
            get {
                return this.max.y - this.min.y;
            }
        }

        public bool IsEmpty {
            get {
                return this.max.x < this.min.x || this.max.y < this.min.y;
            }
        }

        /// <summary>
        /// Returns whether the AABB overlaps with the specified point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Overlaps(int2 point) {
            // Point overlaps if it is within the min and max in all axes
            return this.min.x <= point.x && point.x <= this.max.x && 
                this.min.y <= point.y && point.y <= this.max.y;
        }

        /// <summary>
        /// Returns whether or not this AABB overlaps with the specified other AABB
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Overlaps(AabbInt2 other) {
            // Overlaps if at least one corner overlaps
            if (Overlaps(other.min)) {
                return true;
            }

            if (Overlaps(other.max)) {
                return true;
            }
            
            int2 topLeft = new int2(other.min.x, other.max.y);
            if (Overlaps(topLeft)) {
                return true;
            }

            int2 bottomRight = new int2(other.max.x, other.min.y);
            if (Overlaps(bottomRight)) {
                return true;
            }

            return false;
        }
    }
}