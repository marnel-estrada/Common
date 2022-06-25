using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Populates the dynamic buffer of RequiredCondition elements based on the current goal
    /// of the planner
    /// </summary>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(StartPlanningSystem))]
    public partial class PopulateRequiredConditionsSystem : JobSystemBase {
        private EntityQuery query;
        
        // Type handles
        private ComponentTypeHandle<GoapPlanner> plannerType;
        private BufferTypeHandle<RequiredCondition> requiredConditionType;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner), typeof(RequiredCondition));

            this.plannerType = GetComponentTypeHandle<GoapPlanner>();
            this.requiredConditionType = GetBufferTypeHandle<RequiredCondition>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.plannerType.Update(this);
            this.requiredConditionType.Update(this);
            
            Job job = new Job() {
                plannerType = this.plannerType,
                requiredConditionType = this.requiredConditionType,
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };
            
            return job.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct Job : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<GoapPlanner> plannerType;
            
            public BufferTypeHandle<RequiredCondition> requiredConditionType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                BufferAccessor<RequiredCondition> requiredConditionsList = batchInChunk.GetBufferAccessor(this.requiredConditionType);
                
                // This is used to keep track of actions that were already added
                NativeHashSet<int> addedActions = new NativeHashSet<int>(50, Allocator.Temp);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    GoapPlanner planner = planners[i];
                    if (planner.state != PlanningState.RESOLVING_CONDITIONS) {
                        // No need to continue if planner is no longer resolving conditions
                        continue;
                    }

                    if (planner.currentGoal.IsSome) {
                        // We just reuse the hash set here to avoid frequent memory allocation
                        addedActions.Clear();
                        DynamicBuffer<RequiredCondition> requiredConditions = requiredConditionsList[i];
                        Process(planner, ref requiredConditions, ref addedActions);
                    } else {
                        // Should have a current goal
                        throw new Exception("Planner is trying to plan but doesn't have a current goal.");
                    }
                }
            }

            private void Process(in GoapPlanner planner, ref DynamicBuffer<RequiredCondition> requiredConditions, ref NativeHashSet<int> addedActions) {
                // Clear first
                requiredConditions.Clear();

                GoapAgent agent = this.allAgents[planner.agentEntity];
                GoapDomain domain = agent.Domain;
                
                // Add the goal first since it may have a resolver
                Condition currentGoal = planner.currentGoal.ValueOr(default);
                requiredConditions.Add(new RequiredCondition(currentGoal.id));
                
                // Recurse to all preconditions of the goal until there are no actions left
                AddPreconditions(ref requiredConditions, ref addedActions, domain, currentGoal);
            }

            private void AddPreconditions(ref DynamicBuffer<RequiredCondition> requiredConditions, ref NativeHashSet<int> addedActions, 
                in GoapDomain domain, in Condition effect) {
                // We don't use match here because this needs to fast as much as possible
                ValueTypeOption<FixedList64Bytes<int>> foundActionIndices = domain.GetActionIndices(effect);
                if (foundActionIndices.IsNone) {
                    // No more actions for specified effect
                    return;
                }

                FixedList32Bytes<int> actionIndices = foundActionIndices.ValueOr(default);
                for (int i = 0; i < actionIndices.Length; ++i) {
                    GoapAction action = domain.GetAction(actionIndices[i]);

                    if (addedActions.Contains(action.id)) {
                        // The action was already added. We skip.
                        continue;
                    }
                    
                    AddPreconditions(ref requiredConditions, ref addedActions, action);
                    RecurseThroughPreconditions(ref requiredConditions, ref addedActions, domain, action);
                }
            }

            // Adds the preconditions of the specified action
            private void AddPreconditions(ref DynamicBuffer<RequiredCondition> requiredConditions, ref NativeHashSet<int> addedActions,
                in GoapAction action) {
                ConditionList10 preconditions = action.preconditions;
                for (int i = 0; i < preconditions.Count; ++i) {
                    requiredConditions.Add(new RequiredCondition(preconditions[i].id));
                }

                addedActions.TryAdd(action.id);
            }

            private void RecurseThroughPreconditions(ref DynamicBuffer<RequiredCondition> requiredConditions, ref NativeHashSet<int> addedActions,
                in GoapDomain domain, in GoapAction action) {
                ConditionList10 preconditions = action.preconditions;
                for (int i = 0; i < preconditions.Count; ++i) {
                    AddPreconditions(ref requiredConditions, ref addedActions, domain, preconditions[i]);
                }
            }
        }
    }
}