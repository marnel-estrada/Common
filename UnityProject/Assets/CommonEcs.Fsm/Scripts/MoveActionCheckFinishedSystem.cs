using Unity.Collections;
using Unity.Entities;

namespace Common.Ecs.Fsm {
    [UpdateAfter(typeof(FsmActionStartSystem))]
    [UpdateBefore(typeof(FsmActionEndSystem))]
    [UpdateAfter(typeof(MoveActionSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    class MoveActionCheckFinishedSystem : FsmActionSystem {
        private ArchetypeChunkEntityType entityType;
        private ArchetypeChunkComponentType<MoveAction> moveType;

        protected override EntityQuery ComposeQuery() {
            return GetEntityQuery(typeof(FsmAction), typeof(MoveAction));
        }

        protected override void BeforeChunkTraversal() {
            base.BeforeChunkTraversal();
            
            this.entityType = GetArchetypeChunkEntityType();
            this.moveType = GetArchetypeChunkComponentType<MoveAction>();
        }

        private NativeArray<MoveAction> moveActions;

        protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
            base.BeforeProcessChunk(chunk);
            
            this.moveActions = chunk.GetNativeArray(this.moveType);
        }
        
        // We don't check in DoEnter() because the action has already entered in MoveActionSystem

        protected override void DoUpdate(int index, ref FsmAction fsmAction) {
            CheckAction(index, ref fsmAction);
        }

        private void CheckAction(int index, ref FsmAction fsmAction) {
            if (fsmAction.finished) {
                // Try to send event if already finished
                MoveAction move = this.moveActions[index];
                if (move.finishEvent != Fsm.NULL_EVENT) {
                    // Has a finish event
                    SendEvent(ref fsmAction, move.finishEvent);
                }
            }
        }
    }
}