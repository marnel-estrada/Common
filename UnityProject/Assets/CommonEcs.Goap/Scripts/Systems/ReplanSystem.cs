using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    public class ReplanSystem : JobSystemBase {
        private EntityQuery atomActionsQuery;
        private EntityQuery plannersQuery;

        protected override void OnCreate() {
            this.atomActionsQuery = GetEntityQuery(typeof(AtomAction));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ComponentDataFromEntity<GoapAgent> allAgents = GetComponentDataFromEntity<GoapAgent>();
            
            ResetAtomActionsJob resetAtomActionsJob = new ResetAtomActionsJob() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allAgents = allAgents
            };
            JobHandle handle = resetAtomActionsJob.ScheduleParallel(this.atomActionsQuery, 1, inputDeps);

            ResetGoalIndexJob resetGoalIndexJob = new ResetGoalIndexJob() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(), 
                allAgents = allAgents
            };
            handle = resetGoalIndexJob.ScheduleParallel(this.plannersQuery, 1, handle);
            
            // Reset the replanRequested flag
            handle = this.Entities.ForEach(delegate(ref GoapAgent agent) {
                agent.replanRequested = false;
                
                // We also reset other values here to ensure that they are in correct values
                agent.lastResult = GoapResult.FAILED;
                agent.state = AgentState.IDLE;
            }).ScheduleParallel(handle);
            
            return handle;
        }
        
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
    }
}