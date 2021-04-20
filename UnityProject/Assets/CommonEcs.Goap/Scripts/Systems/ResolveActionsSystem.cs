using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
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
        
        [BurstCompile]
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
                    NativeList<ResolvedAction> actionList = new NativeList<ResolvedAction>(Allocator.Temp);
                    NativeHashSet<int> actionsBeingEvaluated = new NativeHashSet<int>(4, Allocator.Temp);
                    bool result = SearchActions(planner.currentGoal, domain, ref conditionsMap, ref actionList, ref actionsBeingEvaluated);
                    planner.state = result ? PlanningState.SUCCESS : PlanningState.FAILED;

                    // Add the actions to action buffer if search was a success
                    if (result) {
                        AddActions(ref resolvedActions, ref actionList);
                    }
                    
                    // Modify
                    planners[i] = planner;
                }
            }

            // Utility method. Do not remove.
            private static void PrintActions(in GoapPlanner planner, in DynamicBuffer<ResolvedAction> resolvedActions) {
                Debug.Log($"Resolved actions for agent {planner.agentEntity}");
                for (int a = 0; a < resolvedActions.Length; ++a) {
                    Debug.Log(resolvedActions[a].actionId);
                }
            }

            private bool SearchActions(in Condition goal, in GoapDomain domain, ref BoolHashMap conditionsMap, 
                ref NativeList<ResolvedAction> actionList, ref NativeHashSet<int> actionsBeingEvaluated) {
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
                    GoapAction action = domain.GetAction(actionIndices[i]);
                    if (actionsBeingEvaluated.Contains(action.id)) {
                        // This means that the same action is being evaluated while the previous was not yet 
                        // resolved. We skip it as this will cause infinite loop.
                        continue;
                    }

                    actionsBeingEvaluated.TryAdd(action.id);
                    
                    BoolHashMap conditionsMapCopy = conditionsMap;
                    NativeList<ResolvedAction> tempActionList = new NativeList<ResolvedAction>(Allocator.Temp);
                    bool searchSuccess = SearchActionsToSatisfyPreconditions(action, domain, ref conditionsMapCopy, ref tempActionList, ref actionsBeingEvaluated);
                    
                    // We remove here because the action was already searched
                    actionsBeingEvaluated.Remove(action.id);
                    
                    if (!searchSuccess) {
                        continue;
                    }

                    // This means that we found actions to satisfy the goal
                    // We add the effect
                    Condition effect = action.effect;
                    conditionsMapCopy.AddOrSet(effect.id.hashCode, effect.value);
                    
                    // We also copy the effects that the actions needed by the action to satisfy its 
                    // preconditions
                    conditionsMap = conditionsMapCopy;
                    
                    // Add all the actions that were resolved so far
                    actionList.AddRange(tempActionList);
                    
                    return true;
                }

                return false;
            }

            private bool SearchActionsToSatisfyPreconditions(in GoapAction action, in GoapDomain domain,
                ref BoolHashMap conditionsMap, ref NativeList<ResolvedAction> actionList, ref NativeHashSet<int> actionsBeingEvaluated) {
                for (int i = 0; i < action.preconditions.Count; ++i) {
                    Condition precondition = action.preconditions[i];
                    if (!SearchActions(precondition, domain, ref conditionsMap, ref actionList, ref actionsBeingEvaluated)) {
                        // This means that one of the preconditions can't be met by actions
                        return false;
                    }
                }
                
                // At this point, it means that there are actions to satisfy all preconditions
                actionList.Add(new ResolvedAction(action.id, action.atomActionsCount)); // Add the action being searched itself
                
                return true;
            }

            private static void AddActions(ref DynamicBuffer<ResolvedAction> resolvedActions, ref NativeList<ResolvedAction> computedActions) {
                for (int i = 0; i < computedActions.Length; ++i) {
                    resolvedActions.Add(computedActions[i]);
                }
            }
        }
    }
}