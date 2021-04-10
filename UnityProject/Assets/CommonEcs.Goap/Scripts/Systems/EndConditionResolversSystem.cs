using Unity.Entities;

namespace CommonEcs.Goap {
    /// <summary>
    /// Condition resolvers system must run before this system.
    /// This will set to results to the conditionsMap of the planner.
    /// This runs in a single thread. There's no way around it.
    /// </summary>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyConditionsToResolveSystem))]
    public class EndConditionResolversSystem : SystemBase {
        protected override void OnUpdate() {
            ComponentDataFromEntity<GoapPlanner> allPlanners = GetComponentDataFromEntity<GoapPlanner>();
            this.Entities.ForEach(delegate(in ConditionResolver resolver) {
                if (!resolver.resolved) {
                    return;
                }

                GoapPlanner planner = allPlanners[resolver.plannerEntity];
                planner.conditionsMap.AddOrSet(resolver.conditionId.GetHashCode(), resolver.result);
                    
                // Modify
                allPlanners[resolver.plannerEntity] = planner;
            }).Schedule();
            
            // Set planner state to RESOLVING_ACTIONS
            this.Entities.ForEach(delegate(ref GoapPlanner planner) {
                if (planner.state == PlanningState.RESOLVING_CONDITIONS) {
                    planner.state = PlanningState.RESOLVING_ACTIONS;
                }
            }).ScheduleParallel();
        }
    }
}