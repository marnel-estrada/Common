namespace CommonEcs {
    /// <summary>
    /// Contains utility methods related to ComputeBufferSprite
    /// </summary>
    public static class ComputeBufferSpriteUtils {
        /// <summary>
        /// The common way of computing the z position from a layer value.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public static float ComputeZPos(int layer, float yPos) {
            return (-layer * 5) + yPos;
        }
    }
}