using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs.DotsFsm {
    [UpdateBefore(typeof(RemoveStartStateSystem))]
    public class StartFsmSystem : JobSystemBase {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return this.Entities.WithAll<StartState>().ForEach(delegate(Entity entity, in DotsFsmState state) {
                DotsFsm fsm = GetComponent<DotsFsm>(state.fsmOwner);
                fsm.currentState = ValueTypeOption<Entity>.Some(entity);
                SetComponent(state.fsmOwner, fsm); // Modify
            }).Schedule(inputDeps);
            
            // Removal of StartState component is done in RemoveStartStateSystem
        }
    }
}