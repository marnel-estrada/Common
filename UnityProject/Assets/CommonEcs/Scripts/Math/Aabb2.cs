using Unity.Mathematics;

using UnityEngine;

namespace Common {
    public struct Aabb2 {
        private const float BIG_NUMBER = 1e37f;
        private float2 max;
        private float2 min;

        /// <summary>
        /// Constructs an AABB from the specified two vectors.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public Aabb2(float2 v1, float2 v2) {
            this.min = new float2();
            this.max = new float2();
            Empty();

            AddToContain(v1);
            AddToContain(v2);
            Assertion.IsTrue(!this.IsEmpty, "AABB should not be empty when using this constructor.");
        }

        public Aabb2(Rect rect) {
            this.min = new float2();
            this.max = new float2();

            Empty();

            AddToContain(new float2(rect.xMin, rect.yMin));
            AddToContain(new float2(rect.xMax, rect.yMax));
        }

        public void Empty() {
            this.min.x = BIG_NUMBER;
            this.min.y = BIG_NUMBER;

            this.max.x = -BIG_NUMBER;
            this.max.y = -BIG_NUMBER;
        }

        public readonly bool IsEmpty {
            get {
                return this.min.x > this.max.x && this.min.y > this.max.y;
            }
        }

        public readonly float2 Min {
            get {
                return this.min;
            }
        }

        public readonly float2 Max {
            get {
                return this.max;
            }
        }

        public readonly float2 Center {
            get {
                return (this.min + this.max) * 0.5f;
            }
        }

        public readonly float2 Size {
            get {
                return this.max - this.min;   
            }
        }

        public readonly float2 RadiusVector {
            get {
                return this.Size * 0.5f;
            }
        }

        public readonly float Radius {
            get {
                return math.length(this.RadiusVector);
            }
        }

        /// <summary>
        /// Adds the specified vector to the bounding box to contain it.
        /// </summary>
        /// <param name="v"></param>
        public void AddToContain(float2 v) {
            // expand min
            if (v.x < this.min.x) {
                this.min.x = v.x;
            }

            if (v.y < this.min.y) {
                this.min.y = v.y;
            }

            // expand max
            if (v.x > this.max.x) {
                this.max.x = v.x;
            }

            if (v.y > this.max.y) {
                this.max.y = v.y;
            }
        }

        /// <summary>
        /// Returns whether or not the bounding box contains the specified vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public readonly bool Contains(float2 v) {
            return this.min.x.TolerantLesserThanOrEquals(v.x) &&
                v.x.TolerantLesserThanOrEquals(this.max.x) &&
                this.min.y.TolerantLesserThanOrEquals(v.y) &&
                v.y.TolerantLesserThanOrEquals(this.max.y);
        }

        /// <summary>
        /// Returns whether or not this bounding box overlaps with the specified bounding box.
        /// </summary>
        /// <param name="otherBox"></param>
        /// <returns></returns>
        public readonly bool IsOverlapping(in Aabb2 otherBox) {
            // get all corners
            // return true if there is at least one corner that is contained within the bounding box
            return Contains(otherBox.TopLeft) || Contains(otherBox.BottomLeft) ||
                Contains(otherBox.TopRight) || Contains(otherBox.BottomRight);
        }

        /// <summary>
        /// Translates to the position and direction of the specified vector.
        /// </summary>
        /// <param name="translation"></param>
        public void Translate(float2 translation) {
            if (this.IsEmpty) {
                // no need to translate if it is empty
                return;
            }

            // transform to local space
            float2 center = this.Center;
            this.min -= center;
            this.max -= center;

            // translate
            this.min += translation;
            this.max += translation;
        }

        /// <summary>
        /// Returns the unit aabb transformed from this bounding box.
        /// </summary>
        /// <returns></returns>
        public Aabb2 ToLocalSpace() {
            Aabb2 localAabb = new Aabb2();

            float2 center = this.Center;
            localAabb.AddToContain(this.min - center);
            localAabb.AddToContain(this.max - center);

            return localAabb;
        }

        public readonly float2 TopLeft {
            get {
                return new float2(this.min.x, this.max.y);
            }
        }

        public readonly float2 BottomLeft {
            get {
                return this.min;
            }
        }

        public readonly float2 TopRight {
            get {
                return this.max;
            }
        }

        public readonly float2 BottomRight {
            get {
                return new float2(this.max.x, this.min.y);
            }
        }

        public override string ToString() {
            return "min: " + this.min + "; max: " + this.max;
        }
    }
}