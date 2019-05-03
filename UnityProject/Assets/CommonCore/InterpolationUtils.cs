using UnityEngine;
using System.Collections;

/**
 * Contains interpolation-related utility functions.
 */
class InterpolationUtils {

  /**
   * Constructor
   */
  private InterpolationUtils() {
    // can't be instantiated
  }

  /**
   * Linear interpolation.
   */
  public static float Lerp(float from, float to, float t) {
    return from + ((to - from) * t);
  }

  /**
   * Applies a smooth step to the interpolation value.
   */
  public static float SmoothStep(float interpolationValue) {
    return interpolationValue * interpolationValue * (3.0f - (2.0f * interpolationValue));
  }

  /**
   * Parabolic smoothing.
   */
  public static float GetParabolicPos(float t, float y0, float y1, float dh) {
    return GetParabolicPos(t, y0, y1, dh, 0.2f);
  }

  public static float GetParabolicPos(float t, float y0, float y1, float dh, float tFracIncrease) {
    float ret = y0;
    float tF = tFracIncrease;
    float tFs = tF * tF;
    float mH = y0 - dh;
    float A = ((y1 * tF - mH) - y0 * (tF - 1)) / (tF - tFs);
    float B = y1 - y0 - A;
    ret = A * t * t + B * t + y0;
    return ret;
  }

}
