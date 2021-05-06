using Unity.Mathematics;

namespace CommonEcs.Scripts.Math {
    public static class Float3Extensions {
        public static bool TolerantEquals(this float3 a, float3 b) {
            return a.x.TolerantEquals(b.x) && a.y.TolerantEquals(b.y) && a.z.TolerantEquals(b.z);
        } 
    }
}