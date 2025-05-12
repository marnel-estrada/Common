using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.DotsFsm {
    [UpdateInGroup(typeof(DotsFsmSystemGroup))]
    [UpdateAfter(typeof(StartFsmSystem))]
    [UpdateAfter(typeof(ConsumePendingEventSystem))]
    [UpdateAfter(typeof(IdentifyRunningActionsSystem))]
    public abstract partial class DotsFsmActionSystem<TActionType, TActionExecutionType> : JobSystemBase
        where TActionType : unmanaged, IFsmActionComponent 
        where TActionExecutionType : unmanaged, IFsmActionExecution<TActionType> {
        private EntityQuery query;
        private DotsFsmSystemGroup systemGroup;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(DotsFsmAction), typeof(TActionType));
            this.systemGroup = this.World.GetOrCreateSystemManaged<DotsFsmSystemGroup>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            NativeArray<int> chunkBaseEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(
                WorldUpdateAllocator, inputDeps, out JobHandle chunkBaseIndicesHandle);
            inputDeps = JobHandle.CombineDependencies(inputDeps, chunkBaseIndicesHandle);
            
            ExecuteActionJob job = new() {
                chunkBaseEntityIndices = chunkBaseEntityIndices,
                entityHandle = GetEntityTypeHandle(),
                fsmActionHandle = GetComponentTypeHandle<DotsFsmAction>(),
                customActionHandle = GetComponentTypeHandle<TActionType>(),
                execution = PrepareActionExecution()
            };
            
            inputDeps = this.ShouldScheduleParallel ? job.ScheduleParallel(this.query, inputDeps) 
                : job.Schedule(this.query, inputDeps);
            
            // Don't forget to dispose
            inputDeps = chunkBaseEntityIndices.Dispose(inputDeps);
            return inputDeps;
        }

        protected ref NativeReference<bool> RerunGroup {
            get {
                return ref this.systemGroup.RerunGroup;
            }
        }

        protected virtual bool ShouldScheduleParallel {
            get {
                return true;
            }
        }

        protected abstract TActionExecutionType PrepareActionExecution(); 

        [BurstCompile]
        public struct ExecuteActionJob : IJobChunk {
            [ReadOnly]
            public NativeArray<int> chunkBaseEntityIndices;

            [ReadOnly]
            public EntityTypeHandle entityHandle;

            public ComponentTypeHandle<DotsFsmAction> fsmActionHandle;
            public ComponentTypeHandle<TActionType> customActionHandle;

            public TActionExecutionType execution;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityHandle);
                NativeArray<DotsFsmAction> fsmActions = chunk.GetNativeArray(ref this.fsmActionHandle);
                NativeArray<TActionType> customActions = chunk.GetNativeArray(ref this.customActionHandle);
                
                this.execution.BeforeChunkIteration(chunk);

                ChunkEntityEnumeratorWithQueryIndex enumerator = new(
                    useEnabledMask, chunkEnabledMask, chunk.Count, ref this.chunkBaseEntityIndices, unfilteredChunkIndex);
                while (enumerator.NextEntity(out int i, out int queryIndex)) {
                    Entity actionEntity = entities[i];
                    DotsFsmAction fsmAction = fsmActions[i];
                    TActionType customAction = customActions[i];

                    Process(actionEntity, ref fsmAction, ref customAction, queryIndex);
                    
                    // Modify
                    fsmActions[i] = fsmAction;
                    customActions[i] = customAction;
                }
            }
            
            private void Process(in Entity actionEntity, ref DotsFsmAction fsmAction, ref TActionType customAction, int indexInQuery) {
                if (fsmAction.running) {
                    if (!fsmAction.entered) {
                        this.execution.OnEnter(actionEntity, ref fsmAction, ref customAction, indexInQuery);
                        fsmAction.entered = true;
                        fsmAction.exited = false;
                    }

                    // Run OnUpdate()
                    this.execution.OnUpdate(actionEntity, ref fsmAction, ref customAction, indexInQuery);
                } else {
                    if (fsmAction.entered && !fsmAction.exited) {
                        // This means the action's state is no longer the FSM's current state
                        // However, the state entered and hasn't exited yet
                        // We do OnExit()
                        this.execution.OnExit(actionEntity, fsmAction, ref customAction, indexInQuery);
                        fsmAction.exited = true;
                    }
                    
                    // Reset the states
                    fsmAction.entered = false;
                }
            }
        }
    }
}