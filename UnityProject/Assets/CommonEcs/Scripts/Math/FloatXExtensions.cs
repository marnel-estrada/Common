using Common;
using Unity.Mathematics;

namespace CommonEcs.Math {
    /// <summary>
    /// Contains extensions to floatX vectors.
    /// </summary>
    public static class FloatXExtensions {
        // float 2
        public static bool TolerantEquals(this float2 a, float2 b) {
            return a.x.TolerantEquals(b.x) && a.y.TolerantEquals(b.y);
        }

        public static bool IsZero(this float2 v) {
            return Comparison.IsZero(v.x) && Comparison.IsZero(v.y);
        }
        
        // float3
        public static bool TolerantEquals(this float3 a, float3 b) {
            return a.x.TolerantEquals(b.x) && a.y.TolerantEquals(b.y) && a.z.TolerantEquals(b.z);
        }

        public static bool TolerantLessThanOrEquals(this float3 a, float3 b) {
            return a.x.TolerantLesserThanOrEquals(b.x) && 
                   a.y.TolerantLesserThanOrEquals(b.y) &&
                   a.z.TolerantLesserThanOrEquals(b.z);
        }

        public static bool TolerantGreaterThanOrEquals(this float3 a, float3 b) {
            return a.x.TolerantGreaterThanOrEquals(b.x) && 
                   a.y.TolerantGreaterThanOrEquals(b.y) &&
                   a.z.TolerantGreaterThanOrEquals(b.z);
        }

        public static bool IsZero(this float3 v) {
            return Comparison.IsZero(v.x) && Comparison.IsZero(v.y) && Comparison.IsZero(v.z);
        }
        
        // float4
        public static bool TolerantEquals(this float4 a, float4 b) {
            return a.x.TolerantEquals(b.x) 
                   && a.y.TolerantEquals(b.y) 
                   && a.z.TolerantEquals(b.z)
                   && a.w.TolerantEquals(b.w);
        }

        public static bool IsZero(this float4 v) {
            return Comparison.IsZero(v.x) 
                   && Comparison.IsZero(v.y) 
                   && Comparison.IsZero(v.z)
                   && Comparison.IsZero(v.w);
        }
    }
}