using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.Fsm {
    [UpdateAfter(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmActionEndSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public abstract class FsmActionSystem : FsmSystem {
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<FsmAction> fsmActionType;
        
        protected override void BeforeChunkTraversal() {
            this.entityType = GetEntityTypeHandle();
            this.fsmActionType = GetComponentTypeHandle<FsmAction>();
        }

        private NativeArray<Entity> entities;
        private NativeArray<FsmAction> actions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.actions = chunk.GetNativeArray(this.fsmActionType);
        }

        protected override void Process(int index) {
            FsmAction fsmAction = this.actions[index];

            if(!CanExecute(ref fsmAction)) {
                // Can no longer execute based on FSM rules
                this.PostUpdateCommands.DestroyEntity(this.entities[index]);
                return;
            }
            
            // Do enter logic when it is not yet entered
            if (!fsmAction.entered) {
                fsmAction.entered = true;
                this.actions[index] = fsmAction; // Modify
                DoEnter(index, ref fsmAction);
            }
            
            // Note here that we don't destroy the action entity yet
            // This is because the FSM action system might be a system that sends events after an
            // FsmActionJobSystem runs. FsmActionJobSystem can't send events due to the Burst compiler
            // not supporting adding components in EntityCommandBuffer.Concurrent.
            // Maybe DoUpdate() will execute the sending of event

            DoUpdate(index, ref fsmAction);

            // We modify the FsmAction because it might have been modified in DoEnter() or DoUpdate()
            this.actions[index] = fsmAction;
            
            // We don't destroy action entities in FsmActionSystems at all
            // Destruction will be handled by FsmActionEndSystem
        }

        /// <summary>
        /// Enter routines
        /// </summary>
        /// <param name="index"></param>
        protected virtual void DoEnter(int index, ref FsmAction fsmAction) {
        }
        
        /// <summary>
        /// Update routines
        /// </summary>
        /// <param name="index"></param>
        protected virtual void DoUpdate(int index, ref FsmAction fsmAction) {
        }

        protected Entity GetEntityAt(int index) {
            return this.entities[index];
        }
    }
}
