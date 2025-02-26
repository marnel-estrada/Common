using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    /// <summary>
    /// Atom action systems should execute between IdentifyAtomActionsThatCanExecuteSystem and
    /// EndAtomActionsSystem.
    /// </summary>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(ChangeAgentStateToExecutingSystem))]
    public partial class IdentifyAtomActionsThatCanExecuteSystem : JobSystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomAction));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            IdentifyJob identifyJob = new() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                allAgents = GetComponentLookup<GoapAgent>(),
                allDebugEntities = GetComponentLookup<DebugEntity>(),
                allActionSets = GetBufferLookup<ResolvedAction>()
            };
            
            return identifyJob.ScheduleParallel(this.query, inputDeps);
        }
        
        [BurstCompile]
        private struct IdentifyJob : IJobChunk {
            public ComponentTypeHandle<AtomAction> atomActionType;

            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentLookup<DebugEntity> allDebugEntities;
            
            [ReadOnly]
            public BufferLookup<ResolvedAction> allActionSets;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<AtomAction> atomActions = chunk.GetNativeArray(ref this.atomActionType);
                for (int i = 0; i < atomActions.Length; ++i) {
                    AtomAction atomAction = atomActions[i];
                    GoapAgent agent = this.allAgents[atomAction.agentEntity];
                    DebugEntity debugEntity = this.allDebugEntities[atomAction.agentEntity];
                    
                    if (agent.state != AgentState.EXECUTING) {
                        // Agent owner is not executing
                        // We skip
                        continue;
                    }

                    if (atomAction.executing) {
                        if (debugEntity.enabled) {
                            int breakpoint = 0;
                            ++breakpoint;
                        }
                        
                        // The atom action is already currently executing. We should not continue
                        // because if we do, we will call AtomAction.MarkCanExecute() which will reset
                        // the started flag. If we do this, Start() routines will be invoked again
                        // and this is a bug because the action could be stuck.
                        continue;
                    }

                    if (CanExecute(atomAction, agent)) {
                        if (debugEntity.enabled) {
                            int breakpoint = 0;
                            ++breakpoint;
                        }
                        
                        atomAction.MarkCanExecute();
                    } else {
                        // Reset this to false so that other systems that are using this will not
                        // mistake that the atom action is still running.
                        // Only one atom action should run per frame per agent.
                        atomAction.canExecute = false;
                        atomAction.executing = false;
                    }
                    
                    // Modify
                    atomActions[i] = atomAction;
                }
            }

            private bool CanExecute(in AtomAction atomAction, in GoapAgent agent) {
                // Note that the resolved actions are tied up to the planner entity
                DynamicBuffer<ResolvedAction> actionSet = this.allActionSets[agent.plannerEntity];
                int currentActionId = actionSet[agent.currentActionIndex].actionId;
                if (atomAction.actionId != currentActionId) {
                    // Not the action to carry out yet
                    return false;
                }

                // Returns true if the atom action's order is also the same as the agent's
                // currentAtomActionIndex
                return agent.currentAtomActionIndex == atomAction.orderIndex;
            }
        }
    }
}