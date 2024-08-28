namespace CommonEcs {
    /// <summary>
    /// Contains utility methods related to ComputeBufferSprite
    /// </summary>
    public static class ComputeBufferSpriteUtils {
        private const int SPRITE_COUNT_PER_LAYER = 20000;
        
        /// <summary>
        /// The common way of computing the z position from a layer value.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public static float ComputeZPos(int layer, float yPos) {
            return (-layer * 5) + (yPos / SPRITE_COUNT_PER_LAYER);
        }
    }
}