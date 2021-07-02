namespace CommonEcs.Goap {
    public enum AgentState : byte {
        IDLE,
        PLANNING,
        EXECUTING, // Executing actions
        
        // This is the state after agent has executed its current action, failed or success.
        // It's also a mark where atom actions need to revert their state if they need to.
        // Replanning will set the agent's state to this so that atom action can clean up before
        // proceeding to the next plan.
        CLEANUP
    }
}