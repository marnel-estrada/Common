using System;
using Unity.Burst;
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
    public abstract class AtomActionBaseSystem<TActionFilter, TProcessor> : JobSystemBase
        where TActionFilter : struct, IAtomActionComponent
        where TProcessor : struct, IAtomActionProcess<TActionFilter> {
        private EntityQuery query;
        protected bool isActionFilterHasArray;

        protected override void OnCreate() {
            this.query = PrepareQuery();

            // Action has array if it's not zero sized
            this.isActionFilterHasArray = !TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TActionFilter>()).IsZeroSized;
        }

        protected virtual EntityQuery PrepareQuery() {
            return GetEntityQuery(typeof(AtomAction), typeof(TActionFilter));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                actionFilterType = GetComponentTypeHandle<TActionFilter>(),
                isActionFilterHasArray = this.isActionFilterHasArray, // Action filter has array if it's not zero sized
                processor = PrepareProcessor(),
                allAgents = GetComponentDataFromEntity<GoapAgent>(),
                allDebugEntities = GetComponentDataFromEntity<DebugEntity>()
            };

            try {
                JobHandle handle = this.ShouldScheduleParallel ?
                    job.ScheduleParallel(this.query, 1, inputDeps) : job.Schedule(this.query, inputDeps);
                AfterJobScheduling(handle);
                return handle;
            } catch (InvalidOperationException) {
                Debug.LogError(typeof(TActionFilter));
                throw;
            }
        }

        protected virtual void AfterJobScheduling(in JobHandle handle) {
            // Routines like calling AddJobHandleForProducer() may be placed here
        }

        /// <summary>
        /// There may be times that the action system might not want to schedule in parallel
        /// Like for cases when they write using ComponentDataFromEntity
        /// </summary>
        protected virtual bool ShouldScheduleParallel {
            get {
                return true;
            }
        }

        protected ref readonly EntityQuery Query {
            get {
                return ref this.query;
            }
        }

        protected abstract TProcessor PrepareProcessor();

        // We need this to be public so it can be referenced in AssemblyInfo
        [BurstCompile]
        public struct Job : IJobEntityBatchWithIndex {
            public ComponentTypeHandle<AtomAction> atomActionType;
            public ComponentTypeHandle<TActionFilter> actionFilterType;
            public bool isActionFilterHasArray;
            public TProcessor processor;

            [ReadOnly]
            public ComponentDataFromEntity<GoapAgent> allAgents;

            [ReadOnly]
            public ComponentDataFromEntity<DebugEntity> allDebugEntities;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);

                NativeArray<TActionFilter> filterActions = this.isActionFilterHasArray ? batchInChunk.GetNativeArray(this.actionFilterType) : default;
                TActionFilter defaultActionFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)

                this.processor.BeforeChunkIteration(batchInChunk, batchIndex);

                int count = batchInChunk.Count;
                for (int i = 0; i < count; ++i) {
                    AtomAction atomAction = atomActions[i];
                    GoapAgent agent = this.allAgents[atomAction.agentEntity];
                    DebugEntity debug = this.allDebugEntities[atomAction.agentEntity];

                    if (debug.enabled) {
                        int breakpoint = 0;
                        ++breakpoint;
                    }

                    if (agent.state == AgentState.CLEANUP) {
                        // Time to cleanup
                        Cleanup(ref atomAction, ref atomActions, ref filterActions, indexOfFirstEntityInQuery, i);
                        continue;
                    }

                    if (!atomAction.canExecute) {
                        // The current atom action cannot execute yet
                        // Or not yet time to execute
                        continue;
                    }

                    if (this.isActionFilterHasArray) {
                        TActionFilter actionFilter = filterActions[i];
                        ExecuteAction(ref atomAction, ref actionFilter, indexOfFirstEntityInQuery, i);
                        filterActions[i] = actionFilter; // Modify
                    } else {
                        // There's no array for the TActionFilter. It must be a tag component.
                        // Use a default filter component
                        ExecuteAction(ref atomAction, ref defaultActionFilter, indexOfFirstEntityInQuery, i);
                    }

                    atomActions[i] = atomAction; // Modify
                }
            }

            private void Cleanup(ref AtomAction atomAction, ref NativeArray<AtomAction> atomActions, ref NativeArray<TActionFilter> filterActions, int indexOfFirstEntityInQuery, int index) {
                if (this.isActionFilterHasArray) {
                    TActionFilter actionFilter = filterActions[index];
                    this.processor.Cleanup(ref atomAction, ref actionFilter, indexOfFirstEntityInQuery, index);

                    // Modify
                    filterActions[index] = actionFilter;
                } else {
                    // Filter action has no data. Only a tag. We pass default.
                    TActionFilter actionFilter = default;
                    this.processor.Cleanup(ref atomAction, ref actionFilter, indexOfFirstEntityInQuery, index);
                }

                // Modify
                atomActions[index] = atomAction;
            }

            private void ExecuteAction(ref AtomAction atomAction, ref TActionFilter actionFilter, int indexOfFirstEntityInQuery, int index) {
                if (!atomAction.started) {
                    // We call Start() if not yet started
                    atomAction.result = this.processor.Start(ref atomAction, ref actionFilter, indexOfFirstEntityInQuery, index);
                    atomAction.started = true;

                    if (atomAction.result == GoapResult.FAILED || atomAction.result == GoapResult.SUCCESS) {
                        // No need to proceed to update if the Start already failed or succeeded
                        return;
                    }
                }

                atomAction.result = this.processor.Update(ref atomAction, ref actionFilter, indexOfFirstEntityInQuery, index);
            }
        }
    }
}