using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

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
        where TActionFilter : unmanaged, IAtomActionComponent
        where TProcessor : struct, IAtomActionProcess<TActionFilter> {
        private EntityQuery query;
        private bool isActionFilterZeroSized;

        protected override void OnCreate() {
            this.query = PrepareQuery();
            this.isActionFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TActionFilter>()).IsZeroSized;
        }

        protected virtual EntityQuery PrepareQuery() {
            return GetEntityQuery(typeof(AtomAction), typeof(TActionFilter));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                actionFilterType = GetComponentTypeHandle<TActionFilter>(),
                actionFilterHasArray = !this.isActionFilterZeroSized, // Action filter has array if it's not zero sized
                processor = PrepareProcessor()
            };
            
            return this.ShouldScheduleParallel ? 
                job.ScheduleParallel(this.query, 1, inputDeps) : job.Schedule(this.query, inputDeps);
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

        protected abstract TProcessor PrepareProcessor();
        
        // We need this to be public so it can be referenced in AssemblyInfo
        [BurstCompile]
        public struct Job : IJobEntityBatch {
            public ComponentTypeHandle<AtomAction> atomActionType;
            public ComponentTypeHandle<TActionFilter> actionFilterType;
            public bool actionFilterHasArray;
            public TProcessor processor;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);

                NativeArray<TActionFilter> filterActions = this.actionFilterHasArray ? batchInChunk.GetNativeArray(this.actionFilterType) : default;
                TActionFilter defaultActionFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                int count = batchInChunk.Count;
                for (int i = 0; i < count; ++i) {
                    AtomAction atomAction = atomActions[i];
                    if (!atomAction.canExecute) {
                        // The current atom action cannot execute yet
                        // Or not yet time to execute
                        continue;
                    }

                    if (this.actionFilterHasArray) {
                        TActionFilter actionFilter = filterActions[i];
                        ExecuteAction(ref atomAction, ref actionFilter, i);

                        // Modify
                        atomActions[i] = atomAction;
                        filterActions[i] = actionFilter;
                    } else {
                        // There's no array for the TActionFilter. It must be a tag component.
                        // Use a default filter component
                        ExecuteAction(ref atomAction, ref defaultActionFilter, i);

                        // Modify
                        atomActions[i] = atomAction;
                    }
                }
            }

            private void ExecuteAction(ref AtomAction atomAction, ref TActionFilter actionFilter, int index) {
                if (!atomAction.started) {
                    // We call Start() if not yet started
                    atomAction.result = this.processor.Start(ref atomAction, ref actionFilter, index);
                    atomAction.started = true;

                    if (atomAction.result == GoapResult.FAILED || atomAction.result == GoapResult.SUCCESS) {
                        // No need to proceed to update if the Start already failed or succeeded
                        return;
                    }
                }

                atomAction.result = this.processor.Update(ref atomAction, ref actionFilter, index);
            }
        }
    }
}