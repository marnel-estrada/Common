namespace CommonEcs.Goap {
    /// <summary>
    /// This is an action that is used in planning. This is not the actual action that
    /// will be executed.
    /// </summary>
    public readonly struct GoapPlanningAction {
        // We can't use FixedString here to save space when being used in FixedHashMap
        public readonly int id;

        public GoapPlanningAction(int id) {
            this.id = id;
        }
    }
}