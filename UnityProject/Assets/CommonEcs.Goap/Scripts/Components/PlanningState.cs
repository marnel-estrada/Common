namespace CommonEcs.Goap {
    public enum PlanningState : byte {
        RESOLVING_CONDITIONS,
        RESOLVING_ACTIONS,
        SUCCESS,
        FAILED
    }
}