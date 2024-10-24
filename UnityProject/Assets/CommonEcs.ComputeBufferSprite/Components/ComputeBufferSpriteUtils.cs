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
            // The multiplication by 5 here is the scale per z position such that we can
            // add layer order on them.
            return (-layer * 5) + yPos;
        }
    }
}