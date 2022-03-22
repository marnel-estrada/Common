using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    public class ReplanSystem : JobSystemBase {
        private EntityQuery atomActionsQuery;
        private EntityQuery plannersQuery;
        private EntityQuery agentsQuery;

        protected override void OnCreate() {
            this.atomActionsQuery = GetEntityQuery(typeof(AtomAction));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ComponentDataFromEntity<GoapAgent> allAgents = GetComponentDataFromEntity<GoapAgent>();
            
            ResetAtomActionsJob resetAtomActionsJob = new ResetAtomActionsJob() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allAgents = allAgents
            };
            JobHandle handle = resetAtomActionsJob.ScheduleParallel(this.atomActionsQuery, inputDeps);

            ResetGoalIndexJob resetGoalIndexJob = new ResetGoalIndexJob() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(), 
                allAgents = allAgents
            };
            handle = resetGoalIndexJob.ScheduleParallel(this.plannersQuery, handle);

            SetCleanupStateJob setCleanupStateJob = new SetCleanupStateJob() {
                agentType = GetComponentTypeHandle<GoapAgent>()
            };
            handle = setCleanupStateJob.ScheduleParallel(this.agentsQuery, handle);
            
            return handle;
        }
        
        [BurstCompile]
        private struct ResetAtomActionsJob : IJobEntityBatch {
            public ComponentTypeHandle<AtomAction> atomActionType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<AtomAction> actions = batchInChunk.GetNativeArray(this.atomActionType);
                for (int i = 0; i < actions.Length; ++i) {
                    AtomAction action = actions[i];
                    GoapAgent agent = this.allAgents[action.agentEntity];
                    if (!agent.replanRequested) {
                        continue;
                    }

                    // Its agent has replanned
                    // We reset the states such that the action will no longer run
                    action.canExecute = false;
                    action.executing = false;
                    action.started = false;
                    action.result = GoapResult.FAILED;
                    
                    // Modify
                    actions[i] = action;
                }
            }
        }
        
        [BurstCompile]
        private struct ResetGoalIndexJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                for (int i = 0; i < planners.Length; ++i) {
                    GoapPlanner planner = planners[i];
                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    if (!agent.replanRequested) {
                        continue;
                    }
                    
                    // Its agent has replanned
                    // We reset goal index so it will plan for the main goal again
                    planner.goalIndex = 0;
                    
                    // Modify
                    planners[i] = planner;
                }
            }
        }
        
        [BurstCompile]
        private struct SetCleanupStateJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapAgent> agentType;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapAgent> agents = batchInChunk.GetNativeArray(this.agentType);

                for (int i = 0; i < batchInChunk.Count; ++i) {
                    GoapAgent agent = agents[i];
                    if (!agent.replanRequested) {
                        continue;
                    }

                    // We also reset other values here to ensure that they are in correct values
                    // such they would replan again
                    agent.lastResult = GoapResult.FAILED;
                    agent.state = AgentState.CLEANUP;
                
                    agent.replanRequested = false;
                    
                    // Modify
                    agents[i] = agent;
                }
            }
        }
    }
}