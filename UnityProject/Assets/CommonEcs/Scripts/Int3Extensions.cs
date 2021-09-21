using Unity.Mathematics;

namespace CommonEcs {
    public static class Int3Extensions {
        public static int TileDistance(this in int3 self, in int3 other) {
            int xDiff = math.abs(other.x - self.x);
            int yDiff = math.abs(other.y - self.y);
            int zDiff = math.abs(other.z - self.z);

            // We just return the max because agent can move diagonally
            return math.max(math.max(xDiff, yDiff), zDiff);
        }
    }
}