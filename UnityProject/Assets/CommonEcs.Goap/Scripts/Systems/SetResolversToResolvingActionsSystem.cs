using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// This will change the state
    /// of the planner from RESOLVING_CONDITIONS to RESOLVING_ACTIONS.
    /// </summary>
    [UpdateAfter(typeof(EndResolversSystem))]
    public class SetResolversToResolvingActionsSystem : SystemBase {
        protected override void OnUpdate() {
            this.Entities.ForEach(delegate(ref GoapPlanner planner) {
                if (planner.state != PlannerState.RESOLVING_CONDITIONS) {
                    planner.state = PlannerState.RESOLVING_ACTIONS;
                }
            }).ScheduleParallel();
        }
    }
}