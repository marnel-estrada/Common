using System;

using Common;

using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(EndConditionResolversSystem))]
    public partial class ResolveActionsSystem : JobSystemBase {
        private GoapTextDbSystem? textDbSystem;
    
        private EntityQuery query;
        
        // Type handles
        private ComponentTypeHandle<GoapPlanner> plannerType;
        private BufferTypeHandle<ResolvedAction> resolvedActionType;
        private BufferTypeHandle<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucketType;

        protected override void OnCreate() {
            this.textDbSystem = GetOrCreateSystemManaged<GoapTextDbSystem>();
            
            this.query = GetEntityQuery(typeof(GoapPlanner), typeof(ResolvedAction),
                typeof(DynamicBufferHashMap<ConditionHashId, bool>),
                typeof(DynamicBufferHashMap<ConditionHashId, bool>.Entry));

            this.plannerType = GetComponentTypeHandle<GoapPlanner>();
            this.resolvedActionType = GetBufferTypeHandle<ResolvedAction>();
            this.bucketType = GetBufferTypeHandle<DynamicBufferHashMap<ConditionHashId, bool>.Entry>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.textDbSystem == null) {
                throw new CantBeNullException(nameof(this.textDbSystem));
            }
            
            this.plannerType.Update(this);
            this.resolvedActionType.Update(this);
            this.bucketType.Update(this);
            
            ResolveActionsJob job = new() {
                plannerType = this.plannerType,
                resolvedActionType = this.resolvedActionType,
                bucketType = this.bucketType,
                allAgents = GetComponentLookup<GoapAgent>(true),
                allDebug = GetComponentLookup<DebugEntity>(true),
                textResolver = this.textDbSystem.TextResolver
            };

            return job.ScheduleParallel(this.query, inputDeps);
        }

        [BurstCompile]
        private struct ResolveActionsJob : IJobChunk {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            public BufferTypeHandle<ResolvedAction> resolvedActionType;

            [ReadOnly]
            public BufferTypeHandle<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucketType;

            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentLookup<DebugEntity> allDebug;

            // We need this to resolve condition and action names.
            [ReadOnly]
            public GoapTextResolver textResolver;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);
                BufferAccessor<ResolvedAction> resolvedActionBuffers = chunk.GetBufferAccessor(ref this.resolvedActionType);
                BufferAccessor<DynamicBufferHashMap<ConditionHashId, bool>.Entry> buckets = chunk.GetBufferAccessor(ref this.bucketType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
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
#if UNITY_EDITOR
                    if (debug.enabled) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }
#endif

                    // Prepare conditions map. We convert it from the bucket.
                    // The algorithm needs to use BoolHashMap so it can pass the hashmap around. 
                    DynamicBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucket = buckets[i];
                    BoolHashMap boolHashMap = ToBoolHashMap(bucket);

                    NativeList<ResolvedAction> actionList = new(Allocator.Temp);
                    NativeHashSet<int> actionsBeingEvaluated = new(4, Allocator.Temp);
                    Condition currentGoal = planner.currentGoal.ValueOrError();
                    bool result = SearchActions(currentGoal, domain, ref boolHashMap, ref actionList, ref actionsBeingEvaluated, debug.enabled);

                    // Note here that we only set PlanningState to Success if there were actions added to actionList
                    planner.state = result && actionList.Length > 0 ? PlanningState.SUCCESS : PlanningState.FAILED;

                    // Add the actions to action buffer if search was a success
                    if (planner.state == PlanningState.SUCCESS) {
                        AddActions(ref resolvedActions, ref actionList);

#if UNITY_EDITOR
                        if (debug.enabled) {
                            PrintActions(planner, resolvedActions);
                        }
#endif
                    }

#if UNITY_EDITOR
                    if (planner.state == PlanningState.FAILED && debug.enabled) {
                        // Print the failed condition
                        PrintFailedCondition(currentGoal, domain, planner);
                    }
#endif

                    // Modify
                    planners[i] = planner;
                }
            }

            private static BoolHashMap ToBoolHashMap(
                in DynamicBuffer<DynamicBufferHashMap<ConditionHashId, bool>.Entry> bucket) {
                BoolHashMap hashMap = new();

                // Add only those with value
                for (int i = 0; i < bucket.Length; ++i) {
                    DynamicBufferHashMap<ConditionHashId, bool>.Entry entry = bucket[i];
                    if (!entry.HasValue) {
                        // No value
                        continue;
                    }

                    hashMap.AddOrSet(entry.HashCode, entry.Value);
                }

                return hashMap;
            }

            // Utility method. Do not remove.
            private void PrintActions(in GoapPlanner planner, in DynamicBuffer<ResolvedAction> resolvedActions) {
                Debug.Log($"Resolved actions for agent {planner.agentEntity.Index}:{planner.agentEntity.Version}");
                for (int a = 0; a < resolvedActions.Length; ++a) {
                    ResolvedAction resolvedAction = resolvedActions[a];
                    FixedString64Bytes actionName = this.textResolver.GetText(resolvedAction.actionId);
                    Debug.Log($"{actionName}: AtomActionsCount({resolvedAction.atomActionCount})");
                }
            }

            private void PrintFailedCondition(in Condition currentGoal, in GoapDomain domain, in GoapPlanner planner) {
                FixedString64Bytes goalName = this.textResolver.GetText(currentGoal.id.hashCode);
                Debug.Log(
                    $"Failed goal for agent {planner.agentEntity} ({domain.name}): {goalName}.{currentGoal.value}");
            }

            private bool SearchActions(in Condition goal, in GoapDomain domain, ref BoolHashMap conditionsMap,
                ref NativeList<ResolvedAction> actionList, ref NativeHashSet<int> actionsBeingEvaluated, bool isDebug) {
#if UNITY_EDITOR
                FixedString64Bytes goalName = this.textResolver.GetText(goal.id.hashCode);
                if (isDebug) {
                    Debug.Log($"Evaluating goal {goalName}.{goal.value}");
                }
#endif
                
                // Check if goal was already specified in the current conditionsMap
                ValueTypeOption<bool> foundGoalValue = conditionsMap.Find(goal.id.hashCode);
                if (foundGoalValue.IsSome && foundGoalValue.ValueOrError() == goal.value) {
                    // Goal is already satisfied. No need for further search.
#if UNITY_EDITOR
                    if (isDebug) {
                        Debug.Log($"Goal {goalName}.{goal.value} is already satisfied.");
                    }
#endif
                    return true;
                }

                if (foundGoalValue.IsNone && !goal.value) {
                    // This means that the goal is false and is not found in the conditionsMap.
                    // Since conditions default to false, then there's no need to look for actions.
                    // The false goal is already satisfied.
#if UNITY_EDITOR
                    if (isDebug) {
                        Debug.Log(string.Format("Goal {0}.{1} is not found in conditionsMap but is false so it's already satisfied.", 
                            goalName, goal.value));
                    }
#endif
                    return true;
                }
                
#if UNITY_EDITOR
                if (isDebug) {
                    Debug.Log(string.Format("Goal {0}.{1} is not yet satisfied. Proceeding to look for actions.", 
                        goalName, goal.value));
                }
#endif

                ValueTypeOption<FixedList64Bytes<int>> foundActionIndices = domain.GetActionIndices(goal);
                if (foundActionIndices.IsNone) {
                    // There are no actions to satisfy the goal
#if UNITY_EDITOR
                    if (isDebug) {
                        Debug.Log(string.Format("There are no actions to satisfy goal {0}.{1}", goalName, goal.value));
                    }
#endif
                    
                    return false;
                }

                FixedList64Bytes<int> actionIndices = foundActionIndices.ValueOrError();
                for (int i = 0; i < actionIndices.Length; ++i) {
                    GoapAction action = domain.GetAction(actionIndices[i]);
                    if (actionsBeingEvaluated.Contains(action.id)) {
                        // This means that the same action is being evaluated while the previous was not yet 
                        // resolved. We skip it as this will cause infinite loop.
                        continue;
                    }

#if UNITY_EDITOR
                    FixedString64Bytes actionName = this.textResolver.GetText(action.id);
                    if (isDebug) {
                        Debug.Log(string.Format("Evaluating action {0}", actionName));
                    }
#endif

                    actionsBeingEvaluated.TryAdd(action.id);

                    BoolHashMap conditionsMapCopy = conditionsMap;
                    NativeList<ResolvedAction> tempActionList = new NativeList<ResolvedAction>(Allocator.Temp);
                    bool searchSuccess = SearchActionsToSatisfyPreconditions(action, domain, ref conditionsMapCopy, 
                        ref tempActionList, ref actionsBeingEvaluated, isDebug);

                    // We remove here because the action was already searched
                    actionsBeingEvaluated.Remove(action.id);

                    if (!searchSuccess) {
#if UNITY_EDITOR
                        if (isDebug) {
                            Debug.Log(string.Format("Searching for actions for preconditions for {0} failed!", actionName));
                        }
#endif
                        
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
                    actionList.AddRange(tempActionList.AsArray());
                    
#if UNITY_EDITOR
                    if (isDebug) {
                        Debug.Log($"Searching for actions for preconditions for {actionName} succeeded.");
                    }
#endif

                    return true;
                }

                return false;
            }

            private bool SearchActionsToSatisfyPreconditions(in GoapAction action, in GoapDomain domain,
                ref BoolHashMap conditionsMap, ref NativeList<ResolvedAction> actionList, ref NativeHashSet<int> actionsBeingEvaluated, bool isDebug) {
                for (int i = 0; i < action.preconditions.Count; ++i) {
                    Condition precondition = action.preconditions[i];
                    if (SearchActions(precondition, domain, ref conditionsMap, ref actionList,
                            ref actionsBeingEvaluated, isDebug)) {
                        continue;
                    }
                    
                    // This means that one of the preconditions can't be met by actions
#if UNITY_EDITOR
                    if (isDebug) {
                        FixedString64Bytes preconditionName = this.textResolver.GetText(precondition.id.hashCode);
                        Debug.Log(string.Format("SearchActionsToSatisfyPreconditions: Searching for actions for precondition {0}.{1} failed!", 
                            preconditionName, precondition.value));
                    }
#endif
                        
                    return false;
                }

                // At this point, it means that there are actions to satisfy all preconditions
                actionList.Add(new ResolvedAction(action.id, action.atomActionsCount)); // Add the action being searched itself
                
#if UNITY_EDITOR
                if (isDebug) {
                    FixedString64Bytes actionName = this.textResolver.GetText(action.id);
                    Debug.Log(string.Format("SearchActionsToSatisfyPreconditions: Found actions to satisfy preconditions of action {0}.", actionName));
                }
#endif

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