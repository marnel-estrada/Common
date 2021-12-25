using Unity.Mathematics;

namespace CommonEcs.Scripts.Math {
    public static class Float3Extensions {
        public static bool TolerantEquals(this float3 a, float3 b) {
            return a.x.TolerantEquals(b.x) && a.y.TolerantEquals(b.y) && a.z.TolerantEquals(b.z);
        }

        public static bool TolerantLessThanOrEquals(this float3 a, float3 b) {
            return a.x.TolerantLesserThanOrEquals(b.x) && a.y.TolerantLesserThanOrEquals(b.y) && a.z.TolerantLesserThanOrEquals(b.z);
        }

        public static bool TolerantGreaterThanOrEquals(this float3 a, float3 b) {
            return a.x.TolerantGreaterThanOrEquals(b.x) && a.y.TolerantGreaterThanOrEquals(b.y) && a.z.TolerantGreaterThanOrEquals(b.z);
        }

        public static bool IsZero(this float3 v) {
            return Comparison.IsZero(v.x) && Comparison.IsZero(v.y) && Comparison.IsZero(v.z);
        }
    }
}