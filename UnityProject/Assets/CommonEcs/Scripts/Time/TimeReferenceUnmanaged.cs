namespace CommonEcs {
    /// <summary>
    /// Another version of TimeReference that is unmanaged
    /// </summary>
    public struct TimeReferenceUnmanaged {
        private readonly int id;

        public float TimeScale { get; set; }

        public int Id => this.id;

        public TimeReferenceUnmanaged(int id) {
            this.id = id;
            this.TimeScale = 1.0f;
        }

        public float GetScaledTime(float rawDeltaTime) {
            return rawDeltaTime * this.TimeScale;
        }
    }
}