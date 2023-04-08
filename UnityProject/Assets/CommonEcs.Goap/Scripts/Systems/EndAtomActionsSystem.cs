using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs.Goap {
    /// <summary>
    /// Atom actions should execute prior to this system
    ///
    /// Aside from a marker that ends atom actions, it's also the system that copies the atom action result
    /// into the agent result.
    /// </summary>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyAtomActionsThatCanExecuteSystem))]
    public partial class EndAtomActionsSystem : JobSystemBase {
        private EntityQuery atomActionsQuery;
        private EntityQuery agentsQuery;
        private EntityQuery plannersQuery;

        protected override void OnCreate() {
            this.atomActionsQuery = GetEntityQuery(typeof(AtomAction));
            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ComponentTypeHandle<GoapAgent> agentType = GetComponentTypeHandle<GoapAgent>();
            
            ChangeStateFromCleanupToIdleJob changeFromCleanupToIdleJob = new() {
                agentType = agentType
            };
            JobHandle handle = changeFromCleanupToIdleJob.ScheduleParallel(this.agentsQuery, inputDeps);

            ComponentLookup<GoapAgent> allAgents = GetComponentLookup<GoapAgent>();
            
            ProcessAtomActionsJob processAtomActionsJob = new() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allAgents = allAgents
            };
            handle = processAtomActionsJob.ScheduleParallel(this.atomActionsQuery, handle);
            
            // We try to reset the goal index first so that GoapAgent.state remains Executing.
            // Note that it is set to Idle in MoveToNextActionJob when last action failed or ended
            ResetPlannerGoalIndexJob resetGoalIndexJob = new() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(), 
                allAgents = allAgents
            };
            handle = resetGoalIndexJob.ScheduleParallel(this.plannersQuery, handle);

            MoveToNextActionJob moveToNextActionJob = new() {
                agentType = agentType, 
                debugEntityType = GetComponentTypeHandle<DebugEntity>(),
                allActionSets = GetBufferLookup<ResolvedAction>()
            };
            handle = moveToNextActionJob.ScheduleParallel(this.agentsQuery, handle);

            return handle;
        }
        
        /// <summary>
        /// This is the system that changes the agent state from Cleanup to Idle
        /// We added Cleanup state so that actions have a chance to revert state after agent
        /// has completed executing an action.
        ///
        /// We execute this first to mark that the agent's actions have finished cleanup and can
        /// proceed to planning. Note that the jobs after this are the ones setting the state to
        /// Cleanup. After one frame, atom actions should have already executed their cleanup.
        /// </summary>
        [BurstCompile]
        private struct ChangeStateFromCleanupToIdleJob : IJobChunk {
            public ComponentTypeHandle<GoapAgent> agentType;
        
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapAgent> agents = chunk.GetNativeArray(ref this.agentType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    GoapAgent agent = agents[i];
                    if (agent.state != AgentState.CLEANUP) {
                        // Not yet cleaning up. Skip.
                        continue;
                    }

                    agent.state = AgentState.IDLE;
                    agents[i] = agent; // Modify
                } 
            }
        }

        /// <summary>
        /// Routines like setting AtomAction.executing and setting GoapAgent.lastResult is done here
        /// </summary>
        [BurstCompile]
        private struct ProcessAtomActionsJob : IJobChunk {
            public ComponentTypeHandle<AtomAction> atomActionType;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<GoapAgent> allAgents;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<AtomAction> atomActions = chunk.GetNativeArray(ref this.atomActionType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    AtomAction atomAction = atomActions[i];

                    // Apply only to atom actions that were executing
                    if (!atomAction.canExecute) {
                        continue;
                    }

                    GoapAgent agent = this.allAgents[atomAction.agentEntity];
                    agent.lastResult = atomAction.result;
                    
                    // We also set the executing flag so that AtomAction.MarkCanExecute() will not 
                    // be called again in IdentifyAtomActionsThatCanExecuteSystem.
                    // Note here that it is only executing if the last result was Running.
                    // If it was Success or Failed, then the atomAction is already done and
                    // no longer executing.
                    atomAction.executing = atomAction.result == GoapResult.RUNNING;

                    // Modify
                    atomActions[i] = atomAction;
                    this.allAgents[atomAction.agentEntity] = agent;
                }
            }
        }
        
        [BurstCompile]
        private struct ResetPlannerGoalIndexJob : IJobChunk {
            public ComponentTypeHandle<GoapPlanner> plannerType;

            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    GoapPlanner planner = planners[i];
                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    
                    if (agent.state != AgentState.EXECUTING) {
                        // Agent is not executing. We don't need to do anything.
                        continue;
                    }

                    if (agent.lastResult == GoapResult.FAILED || agent.lastResult == GoapResult.SUCCESS) {
                        // This means that the agent was executing and its last result was failed
                        // or success. In other words, the agent has done executing its current
                        // plan. So we reset the planner's goal index so it will plan for the
                        // main goal again.
                        planner.goalIndex = 0;
                        
                        // Modify
                        planners[i] = planner;
                    }
                }
            }
        }
        
        [BurstCompile]
        private struct MoveToNextActionJob : IJobChunk {
            public ComponentTypeHandle<GoapAgent> agentType;

            public ComponentTypeHandle<DebugEntity> debugEntityType;

            [ReadOnly]
            public BufferLookup<ResolvedAction> allActionSets;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapAgent> agents = chunk.GetNativeArray(ref this.agentType);
                NativeArray<DebugEntity> debugEntities = chunk.GetNativeArray(ref this.debugEntityType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    GoapAgent agent = agents[i];
                    if (agent.state != AgentState.EXECUTING) {
                        // Skip agents that are not executing
                        continue;
                    }

                    if (agent.lastResult == GoapResult.RUNNING) {
                        // Can't move or bail if the last result was running
                        continue;
                    }
                    
                    DebugEntity debug = debugEntities[i];

                    if (agent.lastResult == GoapResult.FAILED) {
                        // Bail the subsequent actions
                        // Agent should replan again
                        agent.state = AgentState.CLEANUP;

                        if (debug.enabled) {
                            // ReSharper disable once UseStringInterpolation (due to Burst)
                            Debug.Log(string.Format("Failed at action: {0}, atomAction: {1}", agent.currentActionIndex, 
                                agent.currentAtomActionIndex));
                        }
                    }

                    if (agent.lastResult == GoapResult.SUCCESS) {
                        MoveToNextAction(ref agent, debug);
                    }
                    
                    // Modify
                    agents[i] = agent;
                }
            }

            private void MoveToNextAction(ref GoapAgent agent, in DebugEntity debugEntity) {
                DynamicBuffer<ResolvedAction> actionSet = this.allActionSets[agent.plannerEntity];
                ResolvedAction currentAction = actionSet[agent.currentActionIndex];
                
                if (debugEntity.enabled) {
                    int breakpoint = 0;
                    ++breakpoint;
                }
                
                ++agent.currentAtomActionIndex;
                
                if (debugEntity.enabled) {
                    // ReSharper disable once UseStringInterpolation (due to Burst)
                    Debug.Log(string.Format("Moved atom action index: {0}", agent.currentAtomActionIndex));
                }
                
                if (agent.currentAtomActionIndex < currentAction.atomActionCount) {
                    // This means that there are still more atoms to execute
                    // We can't move to the next action
                    return;
                }

                // At this point, it means that we've reached the end of the atom actions of the action
                // We move to the next action
                ++agent.currentActionIndex;
                
                if (debugEntity.enabled) {
                    // ReSharper disable once UseStringInterpolation (due to Burst)
                    Debug.Log(string.Format("Moved action index: {0}", agent.currentActionIndex));
                }
                
                if (agent.currentActionIndex >= actionSet.Length) {
                    // This means that there are no more actions to execute
                    // Execution is done. We set state to Cleanup so that the agent will plan again.
                    agent.state = AgentState.CLEANUP;
                    return;
                }

                // We set to zero here as it is a new action to execute
                agent.currentAtomActionIndex = 0;
            }
        }
    }
}