using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Common.Math;

namespace Common {
    /// <summary>
    /// An implementation of AABB but using integer coordinates
    /// </summary>
    public class IntAabb2 {

        private readonly IntVector2 min = new IntVector2();
        private readonly IntVector2 max = new IntVector2();

        /// <summary>
        /// Constructor
        /// </summary>
        public IntAabb2() {
            Empty();
        }

        public IntVector2 Min {
            get {
                return min;
            }
        }

        public IntVector2 Max {
            get {
                return max;
            }
        }

        /// <summary>
        /// Empties the bounding box
        /// </summary>
        public void Empty() {
            this.min.Set(int.MaxValue, int.MaxValue);
            this.max.Set(int.MinValue, int.MinValue);
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
        public IntVector2 Center {
            get {
                IntVector2 center = new IntVector2();
                center.x = (this.min.x + this.max.x) >> 1; // Divide 2
                center.y = (this.min.y + this.max.y) >> 1; // Divide 2
                return center;
            }
        }

        /// <summary>
        /// Returns the size of the bounds
        /// </summary>
        public IntVector2 Size {
            get {
                IntVector2 size = new IntVector2();
                size.x = this.max.x - this.min.x;
                size.y = this.max.y - this.min.y;
                return size;
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
                max.y = y;
            }
        }

        /// <summary>
        /// Expands the bounds to contain the specified point
        /// </summary>
        /// <param name="point"></param>
        public void AddToContain(IntVector2 point) {
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
        public bool Contains(IntVector2 point) {
            return Contains(point.x, point.y);
        }

        public IntVector2 TopLeft {
            get {
                return new IntVector2(this.min.x, this.max.y);
            }
        }

        public IntVector2 BottomLeft {
            get {
                // Return a copy because IntVector2 is a class
                // This prevents accidental changes
                return new IntVector2(this.min);
            }
        }

        public IntVector2 TopRight {
            get {
                // Return a copy because IntVector2 is a class
                // This prevents accidental changes
                return new IntVector2(this.max);
            }
        }

        public IntVector2 BottomRight {
            get {
                return new IntVector2(this.max.x, this.min.y);
            }
        }

        /// <summary>
        /// Shrinks the bounds from the left
        /// </summary>
        /// <param name="units"></param>
        public void ShrinkFromLeft(int units) {
            this.min.x = this.min.x + units;

            // Client code should be careful that shrinking bounds should not lead to empty bounds
            Assertion.Assert(!IsEmpty);
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
            Assertion.Assert(!IsEmpty);
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
            Assertion.Assert(!IsEmpty);
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
            Assertion.Assert(!IsEmpty);
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
        public void Copy(IntAabb2 other) {
            if (other != null) {
                this.min.Set(other.min);
                this.max.Set(other.max);
            }
        }

        public override string ToString() {
            return "min: " + min + "; max: " + max;
        }

    }
}
