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

        protected override void OnCreate() {
            this.atomActionsQuery = GetEntityQuery(ComponentType.ReadOnly<AtomAction>());
            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            SetLastResultJob setLastResultJob = new SetLastResultJob() {
                atomActionType = GetComponentTypeHandle<AtomAction>(true),
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };
            JobHandle handle = setLastResultJob.ScheduleParallel(this.atomActionsQuery, 1, inputDeps);

            MoveToNextActionJob moveToNextActionJob = new MoveToNextActionJob() {
                agentType = GetComponentTypeHandle<GoapAgent>(), 
                allActionSets = GetBufferFromEntity<ResolvedAction>()
            };
            handle = moveToNextActionJob.ScheduleParallel(this.agentsQuery, 1, handle);

            return handle;
        }
        
        [BurstCompile]
        private struct SetLastResultJob : IJobEntityBatch {
            [ReadOnly]
            public ComponentTypeHandle<AtomAction> atomActionType;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);
                for (int i = 0; i < atomActions.Length; ++i) {
                    AtomAction atomAction = atomActions[i];

                    // Apply only to atom actions that were executing
                    if (atomAction.canExecute) {
                        GoapAgent agent = this.allAgents[atomAction.agentEntity];
                        agent.lastResult = atomAction.result;

                        // Modify
                        this.allAgents[atomAction.agentEntity] = agent;
                    }
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
    }
}