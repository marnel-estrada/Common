using Unity.Mathematics;

namespace Common {
    /// <summary>
    /// An implementation of AABB but using integer coordinates
    /// </summary>
    public class Int2Aabb {
        private int2 min = new int2();
        private int2 max = new int2();

        /// <summary>
        /// Constructor
        /// </summary>
        public Int2Aabb() {
            Empty();
        }

        public Int2Aabb(int2 min, int2 max) {
            Empty();
            AddToContain(min.x, min.y);
            AddToContain(max.x, max.y);
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

        /// <summary>
        /// Empties the bounding box
        /// </summary>
        public void Empty() {
            this.min = new int2(int.MaxValue, int.MaxValue);
            this.max = new int2(int.MinValue, int.MinValue);
        }

        /// <summary>
        /// Returns whether or not the bounding box is empty.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty {
            get {
                return (this.min.x > this.max.x) && (this.min.y > this.max.y);
            }
        }
        
        /// <summary>
        /// Resolves the center of the bounds
        /// </summary>
        public int2 Center {
            get {
                int2 center;
                center.x = (this.min.x + this.max.x) >> 1; // Divide 2
                center.y = (this.min.y + this.max.y) >> 1; // Divide 2
                return center;
            }
        }
        
        /// <summary>
        /// Returns the size of the bounds
        /// </summary>
        public int2 BoundsSize {
            get {
                int x = this.max.x - this.min.x + 1;
                int y = this.max.y - this.min.y + 1;
                return new int2(x, y);
            }
        }

        /// <summary>
        /// Expands the bounds to contain the specified point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddToContain(int x, int y) {
            // Expand min
            if(x < this.min.x) {
                this.min.x = x;
            }

            if(y < this.min.y) {
                this.min.y = y;
            }

            // Expand max
            if(x > this.max.x) {
                this.max.x = x;
            }

            if(y > this.max.y) {
                this.max.y = y;
            }
        }

        /// <summary>
        /// Expands the bounds to contain the specified point
        /// </summary>
        /// <param name="point"></param>
        public void AddToContain(int2 point) {
            AddToContain(point.x, point.y);
        }

        /// <summary>
        /// Returns whether or not the bounds contains the specified point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Contains(int x, int y) {
            return this.min.x <= x && x <= this.max.x
                && this.min.y <= y && y <= this.max.y;
        }

        /// <summary>
        /// Returns whether or not the bounds contains the specified point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(int2 point) {
            return Contains(point.x, point.y);
        }

        public int2 TopLeft {
            get {
                return new int2(this.min.x, this.max.y);
            }
        }

        public int2 BottomLeft {
            get {
                // Return a copy because IntVector2 is a class
                // This prevents accidental changes
                return new int2(this.min);
            }
        }

        public int2 TopRight {
            get {
                // Return a copy because IntVector2 is a class
                // This prevents accidental changes
                return new int2(this.max);
            }
        }

        public int2 BottomRight {
            get {
                return new int2(this.max.x, this.min.y);
            }
        }

        /// <summary>
        /// Shrinks the bounds from the left
        /// </summary>
        /// <param name="units"></param>
        public void ShrinkFromLeft(int units) {
            this.min.x = this.min.x + units;

            // Client code should be careful that shrinking bounds should not lead to empty bounds
            Assertion.IsTrue(!this.IsEmpty);
        }

        /// <summary>
        /// Expands the bounds from the left
        /// </summary>
        /// <param name="units"></param>
        public void ExpandFromLeft(int units) {
            this.min.x = this.min.x - units;
        }

        /// <summary>
        /// Shrinks the bounds from the right
        /// </summary>
        /// <param name="units"></param>
        public void ShrinkFromRight(int units) {
            this.max.x = this.max.x - units;

            // Client code should be careful that shrinking bounds should not lead to empty bounds
            Assertion.IsTrue(!this.IsEmpty);
        }

        /// <summary>
        /// Expands the bound from the right
        /// </summary>
        /// <param name="units"></param>
        public void ExpandFromRight(int units) {
            this.max.x = this.max.x + units;
        }

        /// <summary>
        /// Shrinks the bounds from the bottom
        /// </summary>
        /// <param name="units"></param>
        public void ShrinkFromBottom(int units) {
            this.min.y = this.min.y + units;

            // Client code should be careful that shrinking bounds should not lead to empty bounds
            Assertion.IsTrue(!this.IsEmpty);
        }

        /// <summary>
        /// Expands the bounds from the bottom
        /// </summary>
        /// <param name="units"></param>
        public void ExpandFromBottom(int units) {
            this.min.y = this.min.y - units;
        }
        
        /// <summary>
        /// Shrinks the bounds from the top
        /// </summary>
        /// <param name="units"></param>
        public void ShrinkFromTop(int units) {
            this.max.y = this.max.y - units;

            // Client code should be careful that shrinking bounds should not lead to empty bounds
            Assertion.IsTrue(!this.IsEmpty);
        }

        /// <summary>
        /// Expands the bounds from the top
        /// </summary>
        /// <param name="units"></param>
        public void ExpandFromTop(int units) {
            this.max.y = this.max.y + units;
        }

        /// <summary>
        /// Copies the specified box
        /// </summary>
        /// <param name="other"></param>
        public void Copy(Int2Aabb other) {
            if (other != null) {
                this.min = other.min;
                this.max = other.max;
            }
        }

        public override string ToString() {
            return "min: " + this.min + "; max: " + this.max;
        }
    }
}
