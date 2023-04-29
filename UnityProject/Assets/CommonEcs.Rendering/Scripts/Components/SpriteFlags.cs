namespace CommonEcs {
    public static class SpriteFlags {
        // Active here means whether or not it is active in SpriteManager or is a recycled instance
        public const int ACTIVE = 0;
        
        public const int VERTICES_CHANGED = 1;
        public const int UV_CHANGED = 2;
        public const int COLOR_CHANGED = 3;
        public const int RENDER_ORDER_CHANGED = 4;

        // We used "Hidden" here instead of "Visible" so we don't have to set the value to true by default
        // which is hard to do for structs
        public const int HIDDEN = 5;
    }
}