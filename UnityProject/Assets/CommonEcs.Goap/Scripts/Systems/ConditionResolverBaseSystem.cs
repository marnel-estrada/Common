using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

namespace CommonEcs.Goap {
    [UpdateInGroup(typeof(GoapSystemGroup))]
    [UpdateAfter(typeof(IdentifyConditionsToResolveSystem))]
    [UpdateBefore(typeof(EndConditionResolversSystem))]
    public abstract partial class ConditionResolverBaseSystem<TResolverFilter, TResolverProcessor> : JobSystemBase
        where TResolverFilter : unmanaged, IConditionResolverComponent
        where TResolverProcessor : struct, IConditionResolverProcess<TResolverFilter> {
        private EntityQuery query;
        protected bool isFilterZeroSized;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver), typeof(TResolverFilter));
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TResolverFilter>()).IsZeroSized;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Job job = new Job() {
                resolverType = GetComponentTypeHandle<ConditionResolver>(),
                filterType = GetComponentTypeHandle<TResolverFilter>(),
                allDebugEntity = GetComponentDataFromEntity<DebugEntity>(),
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = PrepareProcessor()
            };

            try {
                JobHandle handle = this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, inputDeps) :
                    job.Schedule(this.query, inputDeps);
                AfterJobScheduling(handle);
                
                return handle;
            } catch (InvalidOperationException) {
                Debug.LogError(typeof(TResolverFilter));
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

        protected abstract TResolverProcessor PrepareProcessor();
        
        [BurstCompile]
        public struct Job : IJobEntityBatchWithIndex {
            public ComponentTypeHandle<ConditionResolver> resolverType;
            public ComponentTypeHandle<TResolverFilter> filterType;

            [ReadOnly]
            public ComponentDataFromEntity<DebugEntity> allDebugEntity;
            
            public bool filterHasArray;
            public TResolverProcessor processor;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex, int indexOfFirstEntityInQuery) {
                NativeArray<ConditionResolver> resolvers = batchInChunk.GetNativeArray(this.resolverType);
                
                NativeArray<TResolverFilter> filters = this.filterHasArray ? batchInChunk.GetNativeArray(this.filterType) : default;
                TResolverFilter defaultFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                this.processor.BeforeChunkIteration(batchInChunk, batchIndex);
                
                int count = batchInChunk.Count;
                for (int i = 0; i < count; ++i) {
                    ConditionResolver resolver = resolvers[i];
                    if (resolver.resolved) {
                        // Already resolved
                        continue;
                    }

                    if (this.filterHasArray) {
                        TResolverFilter filter = filters[i];
                        ExecuteResolver(ref resolver, ref filter, indexOfFirstEntityInQuery, i);
                        
                        // Modify
                        resolvers[i] = resolver;
                        filters[i] = filter;
                    } else {
                        // There's no array for the TResolverFilter. It must be a tag component.
                        // Use a default filter component
                        ExecuteResolver(ref resolver, ref defaultFilter, indexOfFirstEntityInQuery, i);
                        
                        // Modify
                        resolvers[i] = resolver;
                    }
                }
            }

            private void ExecuteResolver(ref ConditionResolver resolver, ref TResolverFilter resolverFilter, int indexOfFirstEntityInQuery, int iterIndex) {
                resolver.result = this.processor.IsMet(resolver.agentEntity, ref resolverFilter, indexOfFirstEntityInQuery, iterIndex);
                resolver.resolved = true;

#if UNITY_EDITOR
                if (this.allDebugEntity[resolver.agentEntity].enabled) {
                    // Debugging is enabled
                    // ReSharper disable once UseStringInterpolation (due to Burst)
                    Debug.Log(string.Format("Condition {0}: {1}", resolver.id.hashCode, resolver.result));
                }
#endif
            }
        }
    }
}