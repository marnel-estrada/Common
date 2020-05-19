using UnityEngine;
using System.Collections.Generic;

using Common;

public class Aabb2 {

    private Vector2 min;
    private Vector2 max;
    private const float BIG_NUMBER = 1e37f;

    /**
     * Constructor.
     */
    public Aabb2() {
        this.min = new Vector2();
        this.max = new Vector2();
        Empty();
    }

    /**
     * Constructs an AABB from the specified two vectors.
     */
    public Aabb2(Vector2 v1, Vector2 v2) {
        this.min = new Vector2();
        this.max = new Vector2();
        Empty();

        AddToContain(v1);
        AddToContain(v2);
        Assertion.IsTrue(!IsEmpty(), "AABB should not be empty when using this constructor.");
    }

    public Aabb2(Rect rect) {
        this.min = new Vector2();
        this.max = new Vector2();

        Empty();

        AddToContain(new Vector2(rect.xMin, rect.yMin));
        AddToContain(new Vector2(rect.xMax, rect.yMax));
    }

    /**
     * Empties the bounding box.
     */
    public void Empty() {
        this.min.x = BIG_NUMBER;
        this.min.y = BIG_NUMBER;

        this.max.x = -BIG_NUMBER;
        this.max.y = -BIG_NUMBER;
    }

    /**
     * Returns whether or not the bounding box is empty.
     */
    public bool IsEmpty() {
        return (this.min.x > this.max.x) && (this.min.y > this.max.y);
    }

    /**
     * Returns the minimum of the bounding box.
     */
    public Vector2 GetMinimum() {
        return this.min;
    }

    /**
     * Returns the maximum of the bounding box.
     */
    public Vector2 GetMaximum() {
        return this.max;
    }

    /**
     * Returns the center of the bounding box.
     */
    public Vector2 GetCenter() {
        return (this.min + this.max) * 0.5f;
    }

    /**
     * Returns the size vector of the bounding box.
     */
    public Vector2 GetSize() {
        return this.max - this.min;
    }

    /**
     * Returns the radius vector of the bounding box.
     */
    public Vector2 GetRadiusVector() {
        return GetSize() * 0.5f;
    }

    /**
     * Returns the radius of the bounding box.
     */
    public float GetRadius() {
        return GetRadiusVector().magnitude;
    }

    /**
     * Adds the specified vector to the bounding box to contain it.
     */
    public void AddToContain(Vector2 v) {
        // expand min
        if (v.x < min.x) {
            min.x = v.x;
        }

        if (v.y < min.y) {
            min.y = v.y;
        }

        // expand max
        if (v.x > max.x) {
            max.x = v.x;
        }

        if (v.y > max.y) {
            max.y = v.y;
        }
    }

    /**
     * Returns whether or not the bounding box contains the specified vector.
     */
    public bool Contains(Vector2 v) {
        return (Comparison.TolerantLesserThanOrEquals(min.x, v.x) && Comparison.TolerantLesserThanOrEquals(v.x, max.x))
          && (Comparison.TolerantLesserThanOrEquals(min.y, v.y) && Comparison.TolerantLesserThanOrEquals(v.y, max.y));
    }


    /**
     * Returns whether or not this bounding box overlaps with the specified bounding box.
     */
    public bool IsOverlapping(Aabb2 otherBox) {
        // get all corners
        // return true if there is at least one corner that is contained within the bounding box
        return Contains(otherBox.GetTopLeft())
          || Contains(otherBox.GetBottomLeft())
          || Contains(otherBox.GetTopRight())
          || Contains(otherBox.GetBottomRight());
    }

    /**
     * Translates to the position and direction of the specified vector.
     */
    public void Translate(Vector2 translation) {
        if (IsEmpty()) {
            // no need to translate if it is empty
            return;
        }

        // transform to local space
        Vector2 center = GetCenter();
        this.min -= center;
        this.max -= center;

        // translate
        this.min += translation;
        this.max += translation;
    }

    /**
     * Returns the unit aabb transformed from this bounding box.
     */
    public Aabb2 GetAabbInLocalSpace() {
        Aabb2 localAabb = new Aabb2();

        Vector2 center = GetCenter();
        localAabb.AddToContain(this.min - center);
        localAabb.AddToContain(this.max - center);

        return localAabb;
    }

    /**
     * Returns the top left corner of the bounding box.
     */
    public Vector2 GetTopLeft() {
        return new Vector2(min.x, max.y);
    }

    /**
     * Returns the bottom left corner of the bounding box.
     */
    public Vector2 GetBottomLeft() {
        return this.min;
    }

    /**
     * Returns the top right corner of the bounding box.
     */
    public Vector2 GetTopRight() {
        return this.max;
    }

    /**
     * Returns the bottom right corner of the bounding box.
     */
    public Vector2 GetBottomRight() {
        return new Vector2(max.x, min.y);
    }

    /**
     * Return a formatted string.
     */
    public override string ToString() {
        return "min: " + min + "; max: " + max;
    }

}
