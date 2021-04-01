using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Atom action systems should execute between IdentifyAtomActionsThatCanExecuteSystem and
    /// EndAtomActionsSystem.
    /// </summary>
    [UpdateAfter(typeof(ChangeAgentStateToExecutingSystem))]
    public class IdentifyAtomActionsThatCanExecuteSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomAction));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allAgents = GetComponentDataFromEntity<GoapAgent>(),
                allActionSets = GetBufferFromEntity<ResolvedAction>()
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }
        
        private struct Job : IJobEntityBatch {
            public ComponentTypeHandle<AtomAction> atomActionType;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;
            
            [ReadOnly]
            public BufferFromEntity<ResolvedAction> allActionSets;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);
                for (int i = 0; i < atomActions.Length; ++i) {
                    AtomAction atomAction = atomActions[i];
                    GoapAgent agent = this.allAgents[atomAction.agentEntity];
                    if (agent.state != AgentState.EXECUTING) {
                        // Agent owner is not executing
                        // We skip
                        continue;
                    }
                    
                    // Note that the resolved actions are tied up to the planner entity
                    DynamicBuffer<ResolvedAction> actionSet = this.allActionSets[agent.plannerEntity];
                    int currentActionId = actionSet[agent.currentActionIndex].actionId;
                    if (atomAction.actionId != currentActionId) {
                        // Not the action to carry out yet
                        continue;
                    }

                    if (agent.currentAtomActionIndex != atomAction.order) {
                        // Not yet the correct order to execute
                        continue;
                    }
                    
                    // At this point, the atom action can execute
                    atomAction.MarkCanExecute();
                    
                    // Modify
                    atomActions[i] = atomAction;
                }
            }
        }
    }
}