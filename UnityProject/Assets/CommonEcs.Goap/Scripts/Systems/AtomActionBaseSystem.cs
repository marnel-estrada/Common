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
    [UpdateAfter(typeof(IdentifyAtomActionsThatCanExecuteSystem))]
    [UpdateBefore(typeof(EndAtomActionsSystem))]
    public abstract class AtomActionBaseSystem<TActionFilter, TProcessor> : JobSystemBase 
        where TActionFilter : unmanaged, IComponentData
        where TProcessor : unmanaged, IAtomActionProcess<TActionFilter> {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomAction), typeof(TActionFilter));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                actionFilterType = GetComponentTypeHandle<TActionFilter>(),
                processor = PrepareProcessor()
            };
            
            return job.ScheduleParallel(this.query, 1, inputDeps);
        }

        protected abstract TProcessor PrepareProcessor();
        
        // We need this to be public so it can be referenced in AssemblyInfo
        [BurstCompile]
        public struct Job : IJobEntityBatch {
            public ComponentTypeHandle<AtomAction> atomActionType;
            public ComponentTypeHandle<TActionFilter> actionFilterType;
            public TProcessor processor;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<AtomAction> atomActions = batchInChunk.GetNativeArray(this.atomActionType);

                bool hasChunk = batchInChunk.HasChunkComponent(this.actionFilterType);
                NativeArray<TActionFilter> filterActions = hasChunk ? batchInChunk.GetNativeArray(this.actionFilterType) : default;
                TActionFilter defaultActionFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                int count = batchInChunk.Count;
                for (int i = 0; i < count; ++i) {
                    AtomAction atomAction = atomActions[i];
                    if (!atomAction.canExecute) {
                        // The current atom action cannot execute yet
                        // Or not yet time to execute
                        continue;
                    }

                    if (hasChunk) {
                        TActionFilter filterAction = filterActions[i];
                        if (!atomAction.started) {
                            // We call Start() if not yet started
                            atomAction.result = this.processor.Start(ref atomAction, ref filterAction);
                            atomAction.started = true;
                        }

                        atomAction.result = this.processor.Update(ref atomAction, ref filterAction);

                        // Modify
                        atomActions[i] = atomAction;
                        filterActions[i] = filterAction;
                    } else {
                        // There's no chunk for the TActionFilter. It must be a tag component
                        if (!atomAction.started) {
                            // We call Start() if not yet started
                            atomAction.result = this.processor.Start(ref atomAction, ref defaultActionFilter);
                            atomAction.started = true;
                        }

                        atomAction.result = this.processor.Update(ref atomAction, ref defaultActionFilter);

                        // Modify
                        atomActions[i] = atomAction;
                    }
                }
            }
        }
    }
}