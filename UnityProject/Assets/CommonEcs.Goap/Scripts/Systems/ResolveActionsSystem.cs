using System;
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
            this.query = GetEntityQuery(typeof(GoapPlanner), typeof(ResolvedAction),
                typeof(DynamicBufferHashMap<ConditionId, bool>),
                typeof(DynamicBufferHashMap<ConditionId, bool>.Entry<bool>));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                resolvedActionType = GetBufferTypeHandle<ResolvedAction>(),
                bucketType = GetBufferTypeHandle<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>(true),
                allDebug = GetComponentDataFromEntity<DebugEntity>(true)
            };

            return job.ScheduleParallel(this.query, 2, inputDeps);
        }

        [BurstCompile]
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            public BufferTypeHandle<ResolvedAction> resolvedActionType;

            [ReadOnly]
            public BufferTypeHandle<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> bucketType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentDataFromEntity<DebugEntity> allDebug;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                BufferAccessor<ResolvedAction> resolvedActionBuffers = batchInChunk.GetBufferAccessor(this.resolvedActionType);
                BufferAccessor<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> buckets = batchInChunk.GetBufferAccessor(this.bucketType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    GoapPlanner planner = planners[i];
                    if (planner.state != PlanningState.RESOLVING_ACTIONS) {
                        // No need to continue if planner is no longer resolving actions
                        continue;
                    }

                    if (planner.currentGoal.IsNone) {
                        throw new Exception("Trying to plan without a goal.");
                    }

                    DynamicBuffer<ResolvedAction> resolvedActions = resolvedActionBuffers[i];
                    resolvedActions.Clear();

                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    GoapDomain domain = agent.Domain;

                    // Used for debugging
                    DebugEntity debug = this.allDebug[planner.agentEntity];
                    if (debug.enabled) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }

                    // Prepare conditions map. We convert it from the bucket.
                    // The algorithm needs to use BoolHashMap so it can pass the hashmap around. 
                    DynamicBuffer<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> bucket = buckets[i];
                    BoolHashMap boolHashMap = ToBoolHashMap(bucket);

                    NativeList<ResolvedAction> actionList = new NativeList<ResolvedAction>(Allocator.Temp);
                    NativeHashSet<int> actionsBeingEvaluated = new NativeHashSet<int>(4, Allocator.Temp);
                    Condition currentGoal = planner.currentGoal.ValueOrError();
                    bool result = SearchActions(currentGoal, domain, ref boolHashMap, ref actionList, ref actionsBeingEvaluated);

                    // Note here that we only set PlanningState to Success if there were actions added to actionList
                    planner.state = result && actionList.Length > 0 ? PlanningState.SUCCESS : PlanningState.FAILED;

                    // Add the actions to action buffer if search was a success
                    if (planner.state == PlanningState.SUCCESS) {
                        AddActions(ref resolvedActions, ref actionList);

                        if (debug.enabled) {
                            PrintActions(planner, resolvedActions);
                        }
                    }

                    if (planner.state == PlanningState.FAILED && debug.enabled) {
                        // Print the failed condition
                        PrintFailedCondition(currentGoal, domain, planner);
                    }

                    // Modify
                    planners[i] = planner;
                }
            }

            private static BoolHashMap ToBoolHashMap(
                in DynamicBuffer<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> bucket) {
                BoolHashMap hashMap = new BoolHashMap();

                // Add only those with value
                for (int i = 0; i < bucket.Length; ++i) {
                    DynamicBufferHashMap<ConditionId, bool>.Entry<bool> entry = bucket[i];
                    if (!entry.HasValue) {
                        // No value
                        continue;
                    }

                    hashMap.AddOrSet(entry.HashCode, entry.Value);
                }

                return hashMap;
            }

            // Utility method. Do not remove.
            [BurstDiscard]
            private static void PrintActions(in GoapPlanner planner, in DynamicBuffer<ResolvedAction> resolvedActions) {
                Debug.Log($"Resolved actions for agent {planner.agentEntity}");
                for (int a = 0; a < resolvedActions.Length; ++a) {
                    Debug.Log(resolvedActions[a].actionId);
                }
            }

            [BurstDiscard]
            private static void PrintFailedCondition(in Condition currentGoal, in GoapDomain domain, in GoapPlanner planner) {
                Debug.Log($"Failed goal for agent {planner.agentEntity} ({domain.name}): {currentGoal.id.hashCode}");
            }

            private bool SearchActions(in Condition goal, in GoapDomain domain, ref BoolHashMap conditionsMap,
                                       ref NativeList<ResolvedAction> actionList, ref NativeHashSet<int> actionsBeingEvaluated) {
                // Check if goal was already specified in the current conditionsMap
                ValueTypeOption<bool> foundGoalValue = conditionsMap.Find(goal.id.hashCode);
                if (foundGoalValue.IsSome && foundGoalValue.ValueOrError() == goal.value) {
                    // Goal is already satisfied. No need for further search.
                    return true;
                }

                if (foundGoalValue.IsNone && !goal.value) {
                    // This means that the goal is false and is not found in the conditionsMap.
                    // Since conditions default to false, then there's no need to look for actions.
                    // The false goal is already satisfied.
                    return true;
                }

                ValueTypeOption<FixedList64<int>> foundActionIndices = domain.GetActionIndices(goal);
                if (foundActionIndices.IsNone) {
                    // There are no actions to satisfy the goal
                    return false;
                }

                FixedList32<int> actionIndices = foundActionIndices.ValueOrError();
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