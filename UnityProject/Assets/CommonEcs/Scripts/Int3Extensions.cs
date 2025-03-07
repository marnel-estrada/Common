using Unity.Mathematics;

namespace CommonEcs {
    public static class Int3Extensions {
        public static int TileDistance(this int3 self, int3 other) {
            int xDiff = math.abs(other.x - self.x);
            int yDiff = math.abs(other.y - self.y);
            int zDiff = math.abs(other.z - self.z);

            // We just return the max because agent can move diagonally
            return math.max(math.max(xDiff, yDiff), zDiff);
        }

        public static float DistanceSquared(this int3 self, int3 other) {
            return self.x * other.x +
                   self.y * other.y +
                   self.z * other.z;
        }
    }
}