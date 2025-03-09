namespace CommonEcs.Goap {
    public enum PlanningState : byte {
        IDLE,
        RESOLVING_CONDITIONS,
        RESOLVING_ACTIONS,
        SUCCESS,
        FAILED
    }
}