using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(ReplanSystem))]
    public class StartPlanningSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(GoapPlanner),
                typeof(DynamicBufferHashMap<ConditionId, bool>.Entry<bool>));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            StartPlanningJob job = new StartPlanningJob() {
                plannerType = GetComponentTypeHandle<GoapPlanner>(),
                bucketType = GetBufferTypeHandle<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>()
            };

            JobHandle handle = job.ScheduleParallel(this.query, 1, inputDeps);
            
            // Update state of agent
            handle = this.Entities.ForEach(delegate(ref GoapAgent agent) {
                if (agent.state == AgentState.IDLE && agent.goals.Count > 0) {
                    // Agent must have started planning
                    agent.state = AgentState.PLANNING;
                } 
            }).ScheduleParallel(handle);

            return handle;
        }

        [BurstCompile]
        private struct StartPlanningJob : IJobEntityBatch {
            public ComponentTypeHandle<GoapPlanner> plannerType;
            public BufferTypeHandle<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> bucketType;
            
            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<GoapPlanner> planners = batchInChunk.GetNativeArray(this.plannerType);
                BufferAccessor<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> buckets = batchInChunk.GetBufferAccessor(this.bucketType);
                
                for (int i = 0; i < planners.Length; ++i) {
                    GoapPlanner planner = planners[i];
                    
                    GoapAgent agent = this.allAgents[planner.agentEntity];
                    if (agent.state != AgentState.IDLE) {
                        // Maybe the state is currently planning of executing
                        continue;
                    }
                
                    if (agent.goals.Count == 0) {
                        // No goals specified yet
                        continue;
                    }
                
                    DotsAssert.IsTrue(planner.goalIndex >= 0 && planner.goalIndex < agent.goals.Count);
                    planner.StartPlanning(agent.GetGoal(planner.goalIndex));
                    
                    // Reset the condition values
                    DynamicBuffer<DynamicBufferHashMap<ConditionId, bool>.Entry<bool>> bucket = buckets[i];
                    ConditionsMap.ResetValues(ref bucket);
                    
                    // Modify
                    planners[i] = planner;
                }
            }
        }
    }
}