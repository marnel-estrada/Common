using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Unity.Burst;

namespace Common.Ecs.Fsm {
    /// <summary>
    /// Remember that FSM actions written as a job can't send events.
    /// A non job action should come after it so that such system will send such events when needed
    /// </summary>
    /// <typeparam name="ComposerType"></typeparam>
    /// <typeparam name="JobActionType"></typeparam>
    [UpdateBefore(typeof(FsmActionJobSystemBarrier))]
    public abstract class FsmActionJobSystem<ComposerType, JobActionType> : FsmJobSystem
        where ComposerType : struct, IFsmJobActionComposer<JobActionType>
        where JobActionType : struct, IFsmJobAction {
        private EntityQuery query;
        
        protected FsmActionJobSystemBarrier barrier;

        protected override void OnCreate() {
            base.OnCreate();
            this.barrier = this.World.GetOrCreateSystem<FsmActionJobSystemBarrier>();
            this.query = GetQuery();
        }

        /// <summary>
        /// Resolves the job action to be used in the scheduled job
        /// </summary>
        protected abstract ComposerType GetJobActionComposer();

        protected abstract EntityQuery GetQuery();

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            base.OnUpdate(inputDeps);
            
            Job job = new Job {
                allFsms = this.allFsms,
                allStates = this.allStates,
                chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob),
                entityType = GetEntityTypeHandle(),
                actionType = GetComponentTypeHandle<FsmAction>(),
                commandBuffer = this.barrier.CreateCommandBuffer().AsParallelWriter(),
                actionComposer = GetJobActionComposer()
            };

            JobHandle handle = job.Schedule(inputDeps);
            this.barrier.AddJobHandleForProducer(handle);

            return handle;
        }

        // We used an IJob so we can use a writable EntityCommandBuffer
        [BurstCompile]
        private struct Job : IJob {
            public ComponentDataFromEntity<Fsm> allFsms;
            public ComponentDataFromEntity<FsmState> allStates;

            [DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> chunks;

            [ReadOnly]
            public EntityTypeHandle entityType;
            
            public ComponentTypeHandle<FsmAction> actionType;
            
            public EntityCommandBuffer.ParallelWriter commandBuffer; 

            public ComposerType actionComposer;

            public void Execute() {
                for (int i = 0; i < this.chunks.Length; ++i) {
                    Process(this.chunks[i]);
                }
            }
            
            private void Process(ArchetypeChunk chunk) {
                ChunkProcessor processor = new ChunkProcessor() {
                    utility = new FsmActionUtility() {
                        allFsms = this.allFsms,
                        allStates = this.allStates,
                        commandBuffer = this.commandBuffer
                    },
                    entityType = this.entityType,
                    actionType = this.actionType,
                    actionComposer = this.actionComposer
                };
                processor.Init(chunk);
                processor.Execute();
            }
        }

        private struct ChunkProcessor {
            public FsmActionUtility utility;
            
            [ReadOnly]
            public EntityTypeHandle entityType;
            
            public ComponentTypeHandle<FsmAction> actionType;
            
            public ComposerType actionComposer;

            private ArchetypeChunk chunk;
            
            private NativeArray<Entity> entities;
            private NativeArray<FsmAction> actions;

            private JobActionType jobAction;

            public void Init(ArchetypeChunk chunk) {
                this.chunk = chunk;
                this.entities = chunk.GetNativeArray(this.entityType);
                this.actions = chunk.GetNativeArray(this.actionType);
                
                this.jobAction = this.actionComposer.Compose(chunk);
            }

            public void Execute() {
                for (int i = 0; i < this.chunk.Count; ++i) {
                    Process(i);
                }
            }
            
            private void Process(int index) {
                FsmAction fsmAction = this.actions[index];

                if (!CanExecute(ref fsmAction)) {
                    // Can no longer execute based on FSM rules
                    this.utility.commandBuffer.DestroyEntity(index, this.entities[index]);
                    return;
                }

                // Do enter logic when it is not yet entered
                if (!fsmAction.entered) {
                    fsmAction.entered = true;
                    this.actions[index] = fsmAction;
                    
                    this.jobAction.DoEnter(index, ref fsmAction, ref this.utility);
                }

                this.jobAction.DoUpdate(index, ref fsmAction, ref this.utility);

                // We modify the data as DoEnter() and DoUpdate() might have updated the FsmAction instance
                this.actions[index] = fsmAction;
            }
            
            private bool CanExecute(ref FsmAction action) {
                FsmState state = this.utility.allStates[action.stateOwner];
                Fsm fsm = this.utility.allFsms[state.fsmOwner];

                if (fsm.currentEvent != Fsm.NULL_EVENT) {
                    // This means that the FSM has an unconsumed event and must be consumed
                    return false;
                }

                // The action can only execute if the FSMs current state is the state that owned the action
                return fsm.currentState == action.stateOwner;
            }
        }
    }
}