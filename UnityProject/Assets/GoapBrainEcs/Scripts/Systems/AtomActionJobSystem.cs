using CommonEcs;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace GoapBrainEcs {
    /// <summary>
    /// This same as AtomActionComponentSystem but using a job
    /// </summary>
    /// <typeparam name="ActionComponentType"></typeparam>
    /// <typeparam name="AtomActionType"></typeparam>
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    [UpdateBefore(typeof(ReplanAfterFinishSystem))]
    public abstract class AtomActionJobSystem<ActionComponentType, AtomActionType> : JobSystemBase 
        where ActionComponentType : struct, IComponentData
        where AtomActionType : struct, IAtomAction<ActionComponentType> {
        private EntityQuery query;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomAction), typeof(ActionComponentType));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            Job job = new Job() {
                entityTypeHandle = GetEntityTypeHandle(),
                atomActionType = GetComponentTypeHandle<AtomAction>(),
                actionComponentType = GetComponentTypeHandle<ActionComponentType>(),
                chunks = chunks,
                action = PrepareStructAction()
            };

            JobHandle handle = job.Schedule(inputDeps);
            chunks.Dispose(handle);
            
            Dispose(handle);

            return handle;
        }

        protected abstract AtomActionType PrepareStructAction();

        // This is used for cases when the deriving class wants to dispose something after the
        // action job is scheduled. The action might have created arrays that need to be disposed.
        protected virtual void Dispose(JobHandle handle) {
            // May or may not be overridden by deriving class
        }

        // We use IJob here because we don't know if AtomActionType can run in multiple threads
        private struct Job : IJob {
            public EntityTypeHandle entityTypeHandle;
            public ComponentTypeHandle<AtomAction> atomActionType;
            public ComponentTypeHandle<ActionComponentType> actionComponentType;
            public NativeArray<ArchetypeChunk> chunks;

            public AtomActionType action;

            public void Execute() {
                for (int i = 0; i < this.chunks.Length; ++i) {
                    Process(this.chunks[i]);
                }
            }

            private void Process(ArchetypeChunk chunk) {
                NativeArray<Entity> entities = chunk.GetNativeArray(this.entityTypeHandle);
                NativeArray<AtomAction> atomActions = chunk.GetNativeArray(this.atomActionType);
                NativeArray<ActionComponentType> components = chunk.GetNativeArray(this.actionComponentType);

                for (int i = 0; i < chunk.Count; ++i) {
                    Entity actionEntity = entities[i];
                    AtomAction atomAction = atomActions[i];
                    ActionComponentType actionComponent = components[i];
                    
                    // Start
                    if (!atomAction.started) {
                        atomAction.status = this.action.Start(atomAction.agentEntity, actionEntity, ref actionComponent);
                        atomAction.started = true;

                        // Modify
                        atomActions[i] = atomAction;
                        components[i] = actionComponent;
                    }
                    
                    // Action is done if success or failure
                    // Continue to update if running
                    if (atomAction.status == GoapStatus.SUCCESS || atomAction.status == GoapStatus.FAILED) {
                        return;
                    }
                    
                    // Update
                    atomAction.status = this.action.Update(atomAction.agentEntity, actionEntity, ref actionComponent);
                    
                    // Modify
                    atomActions[i] = atomAction;
                    components[i] = actionComponent;
                }
            }
        }
    }
}