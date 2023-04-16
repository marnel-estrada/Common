using System;

using Common;

using Unity.Burst;
using Unity.Burst.Intrinsics;
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
        private GoapTextDbSystem? textDbSystem;
        
        private EntityQuery query;
        protected bool isFilterZeroSized;

        protected override void OnCreate() {
            this.textDbSystem = GetOrCreateSystemManaged<GoapTextDbSystem>();
            
            this.query = GetEntityQuery(typeof(ConditionResolver), typeof(TResolverFilter));
            this.isFilterZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<TResolverFilter>()).IsZeroSized;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.textDbSystem == null) {
                throw new CantBeNullException(nameof(this.textDbSystem));
            }
            
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArray(Allocator.TempJob);
            ExecuteResolversJob job = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                resolverType = GetComponentTypeHandle<ConditionResolver>(),
                filterType = GetComponentTypeHandle<TResolverFilter>(),
                allDebugEntity = GetComponentLookup<DebugEntity>(),
                textResolver = this.textDbSystem.TextResolver,
                filterHasArray = !this.isFilterZeroSized, // Filter has array if it's not zero sized
                processor = PrepareProcessor()
            };

            try {
                JobHandle handle = this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, inputDeps) :
                    job.Schedule(this.query, inputDeps);
                AfterJobScheduling(handle);
                
                // Don't forget to dispose
                handle = chunkBaseEntityIndices.Dispose(handle);
                
                return handle;
            } catch (InvalidOperationException) {
                Debug.LogError(typeof(TResolverFilter));
                throw;
            }
        }

        protected ref readonly GoapTextResolver TextResolver {
            get {
                if (this.textDbSystem == null) {
                    throw new CantBeNullException(nameof(this.textDbSystem));
                }
                
                return ref this.textDbSystem.TextResolver;
            }
        } 
        
        protected virtual void AfterJobScheduling(in JobHandle handle) {
            // Routines like calling AddJobHandleForProducer() may be placed here
        }
        
        /// <summary>
        /// There may be times that the action system might not want to schedule in parallel
        /// Like for cases when they write using ComponentLookup
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
        public struct ExecuteResolversJob : IJobChunk {
            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;
            
            public ComponentTypeHandle<ConditionResolver> resolverType;
            public ComponentTypeHandle<TResolverFilter> filterType;

            [ReadOnly]
            public ComponentLookup<DebugEntity> allDebugEntity;

            [ReadOnly]
            public GoapTextResolver textResolver;
            
            public bool filterHasArray;
            public TResolverProcessor processor;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<ConditionResolver> resolvers = chunk.GetNativeArray(ref this.resolverType);
                
                NativeArray<TResolverFilter> filters = this.filterHasArray ? chunk.GetNativeArray(ref this.filterType) : default;
                TResolverFilter defaultFilter = default; // This will be used if TActionFilter has no chunk (it's a tag component)
                
                this.processor.BeforeChunkIteration(chunk);
                
                ChunkEntityEnumeratorWithQueryIndex enumerator = new(
                    useEnabledMask, chunkEnabledMask, chunk.Count, ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    ConditionResolver resolver = resolvers[i];
                    if (resolver.resolved) {
                        // Already resolved
                        continue;
                    }

                    if (this.filterHasArray) {
                        TResolverFilter filter = filters[i];
                        ExecuteResolver(ref resolver, ref filter, i, queryIndex);
                        
                        // Modify
                        resolvers[i] = resolver;
                        filters[i] = filter;
                    } else {
                        // There's no array for the TResolverFilter. It must be a tag component.
                        // Use a default filter component
                        ExecuteResolver(ref resolver, ref defaultFilter, i, queryIndex);
                        
                        // Modify
                        resolvers[i] = resolver;
                    }
                }
            }

            private void ExecuteResolver(ref ConditionResolver resolver, ref TResolverFilter resolverFilter, int chunkIndex, int queryIndex) {
                resolver.result = this.processor.IsMet(resolver.agentEntity, ref resolverFilter, chunkIndex, queryIndex);
                resolver.resolved = true;

#if UNITY_EDITOR
                if (this.allDebugEntity[resolver.agentEntity].enabled) {
                    // Debugging is enabled
                    FixedString64Bytes conditionName = this.textResolver.GetText(resolver.conditionId.hashCode);
                    Debug.Log(string.Format("Condition {0}: {1}", conditionName, resolver.result));
                }
#endif
            }
        }
    }
}