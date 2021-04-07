using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Atom actions should execute prior to this system
    ///
    /// Aside from a marker that ends atom actions, it's also the system that copies the atom action result
    /// into the agent result.
    /// </summary>
    [UpdateAfter(typeof(IdentifyAtomActionsThatCanExecuteSystem))]
    public class EndAtomActionsSystem : JobSystemBase {
        private EntityQuery atomActionsQuery;
        private EntityQuery agentsQuery;
        private EntityQuery plannersQuery;

        protected override void OnCreate() {
            this.atomActionsQuery = GetEntityQuery(typeof(AtomAction));
            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ProcessAtomActionsJob processAtomActionsJob = new ProcessAtomActionsJob() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };
            JobHandle handle = processAtomActionsJob.ScheduleParallel(this.atomActionsQuery, 1, inputDeps);
            
            // We try to reset the goal index first so that GoapAgent.state remains Executing.
            // Note that it is set to Idle in MoveToNextActionJob when last action failed or ended
            ResetPlannerGoalIndexJob resetGoalIndexJob = new ResetPlannerGoalIndexJob() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(), 
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };
            handle = resetGoalIndexJob.ScheduleParallel(this.plannersQuery, 1, handle);

            MoveToNextActionJob moveToNextActionJob = new MoveToNextActionJob() {
                agentType = GetComponentTypeHandle<GoapAgent>(), 
                allActionSets = GetBufferFromEntity<ResolvedAction>()
            };
            handle = moveToNextActionJob.ScheduleParallel(this.agentsQuery, 1, handle);

            return handle;
        }
        
        /// <summary>
        /// Routines like setting AtomAction.executing and setting GoapAgent.lastResult is done here
        /// </summary>
        [BurstCompile]
        private struct ProcessAtomActionsJob : IJobEntityBatch {
            public ComponentTypeHandle<AtomAction> atomActionType;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);
                for (int i = 0; i < atomActions.Length; ++i) {
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
        private struct MoveToNextActionJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapAgent> agentType;

            [ReadOnly]
            public BufferFromEntity<ResolvedAction> allActionSets;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapAgent> agents = batchInChunk.GetNativeArray(this.agentType);
                for (int i = 0; i < agents.Length; ++i) {
                    GoapAgent agent = agents[i];
                    if (agent.state != AgentState.EXECUTING) {
                        // Skip agents that are not executing
                        continue;
                    }

                    if (agent.lastResult == GoapResult.RUNNING) {
                        // Can't move or bail if the last result was running
                        continue;
                    }

                    if (agent.lastResult == GoapResult.FAILED) {
                        // Bail the subsequent actions
                        // Agent should replan again
                        agent.state = AgentState.IDLE;
                    }

                    if (agent.lastResult == GoapResult.SUCCESS) {
                        MoveToNextAction(ref agent);
                    }
                    
                    // Modify
                    agents[i] = agent;
                }
            }

            private void MoveToNextAction(ref GoapAgent agent) {
                DynamicBuffer<ResolvedAction> actionSet = this.allActionSets[agent.plannerEntity];
                ResolvedAction currentAction = actionSet[agent.currentActionIndex];
                ++agent.currentAtomActionIndex;
                if (agent.currentAtomActionIndex < currentAction.atomActionCount) {
                    // This means that there are still more atoms to execute
                    // We can't move to the next action
                    return;
                }

                // At this point, it means that we've reached the end of the atom actions of the action
                // We move to the next action
                ++agent.currentActionIndex;
                if (agent.currentActionIndex >= actionSet.Length) {
                    // This means that there are no more actions to execute
                    // Execution is done. We set state to Idle so that the agent will plan again.
                    agent.state = AgentState.IDLE;
                    return;
                }

                // We set to zero here as it is a new action to execute
                agent.currentAtomActionIndex = 0;
            }
        }

        [BurstCompile]
        private struct ResetPlannerGoalIndexJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                for (int i = 0; i < batchInChunk.Count; ++i) {
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
    }
}