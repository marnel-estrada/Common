using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    public partial class ReplanSystem : JobSystemBase {
        private EntityQuery atomActionsQuery;
        private EntityQuery plannersQuery;
        private EntityQuery agentsQuery;

        protected override void OnCreate() {
            this.atomActionsQuery = GetEntityQuery(typeof(AtomAction));
            this.plannersQuery = GetEntityQuery(typeof(GoapPlanner));
            this.agentsQuery = GetEntityQuery(typeof(GoapAgent));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            ComponentLookup<GoapAgent> allAgents = SystemAPI.GetComponentLookup<GoapAgent>(true);
            
            ResetAtomActionsJob resetAtomActionsJob = new() {
                atomActionType = SystemAPI.GetComponentTypeHandle<AtomAction>(),
                allAgents = allAgents
            };
            JobHandle resetAtomActionsHandle = resetAtomActionsJob.ScheduleParallel(this.atomActionsQuery, inputDeps);

            ResetGoalIndexJob resetGoalIndexJob = new() {
                plannerType = SystemAPI.GetComponentTypeHandle<GoapPlanner>(), 
                allAgents = allAgents
            };
            JobHandle resetGoalIndexHandle = resetGoalIndexJob.ScheduleParallel(this.plannersQuery, inputDeps);

            // We can combine these two since it doesn't touch GoapAgent
            inputDeps = JobHandle.CombineDependencies(resetAtomActionsHandle, resetGoalIndexHandle);
            
            SetCleanupStateJob setCleanupStateJob = new() {
                agentType = SystemAPI.GetComponentTypeHandle<GoapAgent>()
            };
            inputDeps = setCleanupStateJob.ScheduleParallel(this.agentsQuery, inputDeps);

            return inputDeps;
        }
        
        [BurstCompile]
        private struct ResetAtomActionsJob : IJobChunk {
            public ComponentTypeHandle<AtomAction> atomActionType;

            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<AtomAction> actions = chunk.GetNativeArray(ref this.atomActionType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
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
        private struct ResetGoalIndexJob : IJobChunk {
            public ComponentTypeHandle<GoapPlanner> plannerType;

            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapPlanner> planners = chunk.GetNativeArray(ref this.plannerType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
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
        private struct SetCleanupStateJob : IJobChunk {
            public ComponentTypeHandle<GoapAgent> agentType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<GoapAgent> agents = chunk.GetNativeArray(ref this.agentType);

                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
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