using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateAfter(typeof(EndConditionResolversSystem))]
    public class ResolveActionsSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner), typeof(ResolvedAction));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                resolvedActionType = GetBufferTypeHandle<ResolvedAction>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>(true)
            };
            
            return job.ScheduleParallel(this.query, 2, inputDeps);
        }
        
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            public BufferTypeHandle<ResolvedAction> resolvedActionType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                BufferAccessor<ResolvedAction> resolvedActionBuffers = batchInChunk.GetBufferAccessor(this.resolvedActionType);
                
                for (int i = 0; i < batchInChunk.Count; ++i) {
                    GoapPlanner planner = planners[i];
                    if (planner.state != PlanningState.RESOLVING_ACTIONS) {
                        // No need to continue if planner is no longer resolving actions
                        continue;
                    }
                    
                    DynamicBuffer<ResolvedAction> resolvedActions = resolvedActionBuffers[i];
                    resolvedActions.Clear();

                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    GoapDomain domain = agent.Domain;
                    BoolHashMap conditionsMap = planner.conditionsMap;
                    NativeList<int> actionList = new NativeList<int>(Allocator.Temp);
                    bool result = SearchActions(planner.currentGoal, domain, ref conditionsMap, ref actionList);
                    planner.state = result ? PlanningState.SUCCESS : PlanningState.FAILED;

                    // Add the actions to action buffer if search was a success
                    if (result) {
                        AddActions(ref resolvedActions, ref actionList);
                    }
                    
                    // Modify
                    planners[i] = planner;
                }
            }

            private bool SearchActions(in Condition goal, in GoapDomain domain, ref BoolHashMap conditionsMap, 
                ref NativeList<int> actionList) {
                // Check if goal was already specified in the current conditionsMap
                ValueTypeOption<bool> foundGoalValue = conditionsMap.Find(goal.id.hashCode);
                if (foundGoalValue.IsSome && foundGoalValue.ValueOr(default) == goal.value) {
                    // Goal is already satisfied. No need for further search.
                    return true;
                }

                ValueTypeOption<FixedList32<int>> foundActionIndices = domain.GetActionIndices(goal);
                if (foundActionIndices.IsNone) {
                    // There are no actions to satisfy the goal
                    return false;
                }

                FixedList32<int> actionIndices = foundActionIndices.ValueOr(default);
                for (int i = 0; i < actionIndices.Length; ++i) {
                    GoapPlanningAction action = domain.GetAction(actionIndices[i]);
                    BoolHashMap conditionsMapCopy = conditionsMap;
                    NativeList<int> tempActionList = new NativeList<int>(Allocator.Temp);
                    if (SearchActionsToSatisfyPreconditions(action, domain, ref conditionsMapCopy, ref tempActionList)) {
                        // This means that we found actions to satisfy the goal
                        // We apply the newly found goals to the original so they will be considered in future
                        // searches
                        conditionsMap = conditionsMapCopy;
                        actionList.AddRange(tempActionList);
                        return true;
                    }
                }

                return false;
            }

            private bool SearchActionsToSatisfyPreconditions(in GoapPlanningAction action, in GoapDomain domain,
                ref BoolHashMap conditionsMap, ref NativeList<int> actionList) {
                BoolHashMap conditionsMapCopy = conditionsMap;
                NativeList<int> tempActionList = new NativeList<int>(Allocator.Temp);
                
                for (int i = 0; i < action.preconditions.Count; ++i) {
                    Condition precondition = action.preconditions[i];
                    if (!SearchActions(precondition, domain, ref conditionsMapCopy, ref tempActionList)) {
                        // This means that one of the preconditions can't be met by actions
                        return false;
                    }
                }
                
                // At this point, it means that there are actions to satisfy all preconditions
                // We propagate to callee
                conditionsMap = conditionsMapCopy;
                
                // Add actions to satisfy the preconditions
                actionList.AddRange(tempActionList);
                actionList.Add(action.id); // Add the action being search itself
                
                return true;
            }

            private static void AddActions(ref DynamicBuffer<ResolvedAction> resolvedActions, ref NativeList<int> computedActions) {
                for (int i = 0; i < computedActions.Length; ++i) {
                    resolvedActions.Add(new ResolvedAction(computedActions[i]));
                }
            }
        }
    }
}