﻿using  Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// This is job system version of FsmStatePreparationSystem
    /// </summary>
    [UpdateAfter(typeof(FsmConsumeEventSystem))]
    [UpdateBefore(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmStatePreparationJobSystemBarrier))]
    [UpdateBefore(typeof(FsmResetEventSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class FsmStatePreparationJobSystem<TTagType, TPrepareActionType> : JobComponentSystem
        where TTagType : struct, IComponentData
        where TPrepareActionType : struct, IFsmStatePreparationAction {
        protected FsmStatePreparationJobSystemBarrier barrier;

        private EntityQuery query;
        private ArchetypeChunkComponentType<FsmState> stateType;

        protected ComponentDataFromEntity<Fsm> allFsms;

        protected override void OnCreate() {
            this.barrier = this.World.GetOrCreateSystem<FsmStatePreparationJobSystemBarrier>();
            this.query = GetEntityQuery(typeof(FsmState), typeof(StateJustTransitioned), 
                typeof(TTagType));
        }

        protected abstract TPrepareActionType StatePreparationAction { get; }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            this.allFsms = GetComponentDataFromEntity<Fsm>();
            
            Job job = new Job {
                stateType = GetArchetypeChunkComponentType<FsmState>(), 
                commandBuffer = this.barrier.CreateCommandBuffer().ToConcurrent(), 
                preparationAction = this.StatePreparationAction
            };

            JobHandle jobHandle = JobChunkExtensions.Schedule(job, this.query, inputDeps);
            this.barrier.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }

        private struct Job : IJobChunk {
            public ArchetypeChunkComponentType<FsmState> stateType;
            public EntityCommandBuffer.Concurrent commandBuffer;
            public TPrepareActionType preparationAction;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<FsmState> states = chunk.GetNativeArray(this.stateType);
                for (int i = 0; i < chunk.Count; ++i) {
                    FsmState state = states[i];
                    this.preparationAction.Prepare(ref state, ref this.commandBuffer, i);

                    // Remove StateJustTransitioned so it will not be processed again
                    this.commandBuffer.RemoveComponent<StateJustTransitioned>(i, state.entityOwner);
                }
            }
        }
    }
}
