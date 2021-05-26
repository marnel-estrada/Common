using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyConditionsToResolveSystem))]
    [UpdateBefore(typeof(EndConditionResolversSystem))]
    public abstract class ConditionResolverBaseSystem<TResolverFilter, TResolverProcessor> : JobSystemBase
        where TResolverFilter : unmanaged, IConditionResolverComponent
        where TResolverProcessor : struct, IConditionResolverProcess<TResolverFilter> {
        private EntityQuery query;
        private bool isFilterZeroSized;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver), typeof(TResolverFilter));
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TResolverFilter>()).IsZeroSized;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                resolverType = GetComponentTypeHandle<ConditionResolver>(),
                filterType = GetComponentTypeHandle<TResolverFilter>(),
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = PrepareProcessor()
            };

            return this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, 1, inputDeps) :
                job.Schedule(this.query, inputDeps);
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
        
        protected abstract TResolverProcessor PrepareProcessor();
        
        [BurstCompile]
        public struct Job : IJobEntityBatch {
            public ComponentTypeHandle<ConditionResolver> resolverType;
            public ComponentTypeHandle<TResolverFilter> filterType;
            public bool filterHasArray;
            public TResolverProcessor processor;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex) {
                NativeArray<ConditionResolver> resolvers = batchInChunk.GetNativeArray(this.resolverType);
                
                NativeArray<TResolverFilter> filters = this.filterHasArray ? batchInChunk.GetNativeArray(this.filterType) : default;
                TResolverFilter defaultFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                int count = batchInChunk.Count;
                for (int i = 0; i < count; ++i) {
                    ConditionResolver resolver = resolvers[i];
                    if (resolver.resolved) {
                        // Already resolved
                        continue;
                    }

                    if (this.filterHasArray) {
                        TResolverFilter filter = filters[i];
                        ExecuteResolver(ref resolver, ref filter);
                        
                        // Modify
                        resolvers[i] = resolver;
                        filters[i] = filter;
                    } else {
                        // There's no array for the TResolverFilter. It must be a tag component.
                        // Use a default filter component
                        ExecuteResolver(ref resolver, ref defaultFilter);
                        
                        // Modify
                        resolvers[i] = resolver;
                    }
                }
            }

            private void ExecuteResolver(ref ConditionResolver resolver, ref TResolverFilter resolverFilter) {
                resolver.result = this.processor.IsMet(resolver.agentEntity, ref resolverFilter);
                resolver.resolved = true;
            }
        }
    }
}