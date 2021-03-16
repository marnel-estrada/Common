using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Populates the dynamic buffer of RequiredCondition elements based on the current goal
    /// of the planner
    /// </summary>
    public class PopulateRequiredConditionsSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner), typeof(RequiredCondition));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                requiredConditionType = GetBufferTypeHandle<RequiredCondition>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        private struct Job : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<GoapPlanner> plannerType;
            
            public BufferTypeHandle<RequiredCondition> requiredConditionType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                BufferAccessor<RequiredCondition> requiredConditionsList = batchInChunk.GetBufferAccessor(this.requiredConditionType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    GoapPlanner planner = planners[i];
                    if (planner.state != PlannerState.RESOLVING_CONDITIONS) {
                        // No need to continue if planner is no longer resolving conditions
                        continue;
                    }
                    
                    DynamicBuffer<RequiredCondition> requiredConditions = requiredConditionsList[i];
                    Process(planner, ref requiredConditions);
                }
            }

            private void Process(in GoapPlanner planner, ref DynamicBuffer<RequiredCondition> requiredConditions) {
                // Clear first
                requiredConditions.Clear();

                GoapAgent agent = this.allAgents[planner.agentEntity];
                GoapDomain domain = agent.Domain;
                
                // Add the goal first since it may have a resolver
                requiredConditions.Add(new RequiredCondition(planner.goal.id));
                
                // Recurse to all preconditions of the goal until there are no actions left
                AddPreconditions(ref requiredConditions, domain, planner.goal);
            }

            private void AddPreconditions(ref DynamicBuffer<RequiredCondition> requiredConditions, in GoapDomain domain, in Condition effect) {
                // We don't use match here because this needs to fast as much as possible
                ValueTypeOption<FixedList32<int>> foundActionIndices = domain.GetActionIndices(effect);
                if (foundActionIndices.IsNone) {
                    // No more actions for specified effect
                    return;
                }

                FixedList32<int> actionIndices = foundActionIndices.ValueOr(default);
                for (int i = 0; i < actionIndices.Length; ++i) {
                    GoapPlanningAction action = domain.GetAction(actionIndices[i]);
                    AddPreconditions(ref requiredConditions, action);
                    RecurseThroughPreconditions(ref requiredConditions, domain, action);
                }
            }

            // Adds the preconditions of the specified action
            private void AddPreconditions(ref DynamicBuffer<RequiredCondition> requiredConditions,
                in GoapPlanningAction action) {
                ConditionList10 preconditions = action.preconditions;
                for (int i = 0; i < preconditions.Count; ++i) {
                    requiredConditions.Add(new RequiredCondition(preconditions[i].id));
                }
            }

            private void RecurseThroughPreconditions(ref DynamicBuffer<RequiredCondition> requiredConditions,
                in GoapDomain domain, in GoapPlanningAction action) {
                ConditionList10 preconditions = action.preconditions;
                for (int i = 0; i < preconditions.Count; ++i) {
                    AddPreconditions(ref requiredConditions, domain, preconditions[i]);
                }
            }
        }
    }
}