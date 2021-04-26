using Unity.Mathematics;

namespace CommonEcs {
    /// <summary>
    /// This is to differentiate world coordinates from grid coordinates.
    /// </summary>
    public struct WorldCoord3 {
        public int3 value;

        public WorldCoord3(int3 value) {
            this.value = value;
        }
    }
}