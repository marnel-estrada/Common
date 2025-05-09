using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CommonEcs.Goap {
    /// <summary>
    /// An abstract base class for atom action systems
    /// </summary>
    /// <typeparam name="TActionFilter"></typeparam>
    /// <typeparam name="TProcessor"></typeparam>
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyAtomActionsThatCanExecuteSystem))]
    [UpdateBefore(typeof(EndAtomActionsSystem))]
    public abstract partial class AtomActionBaseSystem<TActionFilter, TProcessor> : JobSystemBase
        where TActionFilter : unmanaged, IAtomActionComponent
        where TProcessor : unmanaged, IAtomActionProcess<TActionFilter> {
        private EntityQuery query;
        protected bool isActionFilterHasArray;

        protected override void OnCreate() {
            this.query = PrepareQuery();
            RequireForUpdate(this.query);

            // Action has array if it's not zero sized
            this.isActionFilterHasArray = !TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TActionFilter>()).IsZeroSized;
        }

        protected virtual EntityQuery PrepareQuery() {
            return GetEntityQuery(typeof(AtomAction), typeof(TActionFilter));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArray(WorldUpdateAllocator);
            ExecuteAtomActionJob job = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                actionFilterType = GetComponentTypeHandle<TActionFilter>(),
                isActionFilterHasArray = this.isActionFilterHasArray, // Action filter has array if it's not zero sized
                processor = PrepareProcessor(),
                allAgents = GetComponentLookup<GoapAgent>(),
                allDebugEntities = GetComponentLookup<DebugEntity>()
            };

            try {
                inputDeps = this.ShouldScheduleParallel ?
                    job.ScheduleParallel(this.query, inputDeps) : job.Schedule(this.query, inputDeps);
                AfterJobScheduling(inputDeps);
                
                // Don't forget to dispose
                inputDeps = chunkBaseEntityIndices.Dispose(inputDeps);
                
                return inputDeps;
            } catch (InvalidOperationException e) {
                Debug.LogError(typeof(TActionFilter));
                Debug.LogError(e.StackTrace);
                throw;
            }
        }

        protected virtual void AfterJobScheduling(in JobHandle handle) {
            // Routines like calling AddJobHandleForProducer() may be placed here
        }

        /// <summary>
        /// There may be times that the action system might not want to schedule in parallel
        /// Like for cases when they write using ComponentLookup
        /// </summary>
        protected virtual bool ShouldScheduleParallel => true;

        protected ref EntityQuery Query => ref this.query;

        protected abstract TProcessor PrepareProcessor();

        // We need this to be public so it can be referenced in AssemblyInfo
        [BurstCompile]
        public struct ExecuteAtomActionJob : IJobChunk {
            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;
            
            public ComponentTypeHandle<AtomAction> atomActionType;
            public ComponentTypeHandle<TActionFilter> actionFilterType;
            public bool isActionFilterHasArray;
            public TProcessor processor;

            [ReadOnly]
            public ComponentLookup<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentLookup<DebugEntity> allDebugEntities;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<AtomAction> atomActions = chunk.GetNativeArray(ref this.atomActionType);

                NativeArray<TActionFilter> filterActions = this.isActionFilterHasArray ? chunk.GetNativeArray(ref this.actionFilterType) : default;
                TActionFilter defaultActionFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)

                this.processor.BeforeChunkIteration(chunk);
                
                ChunkEntityEnumeratorWithQueryIndex enumerator = new(
                    useEnabledMask, chunkEnabledMask, chunk.Count, ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    AtomAction atomAction = atomActions[i];
                    GoapAgent agent = this.allAgents[atomAction.agentEntity];
                    DebugEntity debug = this.allDebugEntities[atomAction.agentEntity];

                    if (debug.enabled) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }

                    if (agent.state == AgentState.CLEANUP) {
                        // Time to cleanup
                        Cleanup(ref atomAction, ref atomActions, ref filterActions, i, queryIndex);
                        continue;
                    }

                    if (!atomAction.canExecute) {
                        // The current atom action cannot execute yet
                        // Or not yet time to execute
                        continue;
                    }

                    if (this.isActionFilterHasArray) {
                        TActionFilter actionFilter = filterActions[i];
                        ExecuteAction(ref atomAction, ref actionFilter, i, queryIndex);
                        filterActions[i] = actionFilter; // Modify
                    } else {
                        // There's no array for the TActionFilter. It must be a tag component.
                        // Use a default filter component
                        ExecuteAction(ref atomAction, ref defaultActionFilter, i, queryIndex);
                    }

                    atomActions[i] = atomAction; // Modify
                }
            }

            private void Cleanup(ref AtomAction atomAction, ref NativeArray<AtomAction> atomActions, ref NativeArray<TActionFilter> filterActions, int chunkIndex, int queryIndex) {
                if (this.isActionFilterHasArray) {
                    TActionFilter actionFilter = filterActions[chunkIndex];
                    this.processor.Cleanup(ref atomAction, ref actionFilter, chunkIndex, queryIndex);

                    // Modify
                    filterActions[chunkIndex] = actionFilter;
                } else {
                    // Filter action has no data. Only a tag. We pass default.
                    TActionFilter actionFilter = default;
                    this.processor.Cleanup(ref atomAction, ref actionFilter, chunkIndex, queryIndex);
                }

                // Modify
                atomActions[chunkIndex] = atomAction;
            }

            private void ExecuteAction(ref AtomAction atomAction, ref TActionFilter actionFilter, int chunkIndex, int queryIndex) {
                if (!atomAction.started) {
                    // We call Start() if not yet started
                    atomAction.result = this.processor.Start(ref atomAction, ref actionFilter, chunkIndex, queryIndex);
                    atomAction.started = true;

                    if (atomAction.result == GoapResult.FAILED || atomAction.result == GoapResult.SUCCESS) {
                        // No need to proceed to update if the Start already failed or succeeded
                        return;
                    }
                }

                atomAction.result = this.processor.Update(ref atomAction, ref actionFilter, chunkIndex, queryIndex);
            }
        }
    }
}