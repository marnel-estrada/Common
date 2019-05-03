using Unity.Mathematics;

namespace CommonEcs {
    public static class Int2Extensions {
        public static bool Equals(this int2 source, int2 other) {
            bool2 result = source == other;
            return result.x && result.y;
        }
    }
}