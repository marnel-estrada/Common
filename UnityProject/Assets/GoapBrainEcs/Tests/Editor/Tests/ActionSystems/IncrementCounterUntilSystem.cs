using GoapBrainEcs;

using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    [UpdateBefore(typeof(ReplanAfterFinishSystem))]
    public class IncrementCounterUntilSystem : AtomActionComponentSystem<IncrementCounterUntil> {        
        private ComponentDataFromEntity<Counter> allCounters;

        protected override void OnUpdate() {
            this.allCounters = GetComponentDataFromEntity<Counter>();
            
            // Use parent's update
            base.OnUpdate();
        }

        protected override GoapStatus Start(ref AtomAction atomAction, ref IncrementCounterUntil actionComponent) {
            // We return return running so it will continue to Update()
            return GoapStatus.RUNNING;
        }

        protected override GoapStatus Update(ref AtomAction atomAction, ref IncrementCounterUntil actionComponent) {
            Counter counter = this.allCounters[atomAction.agentEntity];
            counter.value += 1;
            this.allCounters[atomAction.agentEntity] = counter; // Modify data
            
            // Return success when counter value has been reached
            return counter.value >= actionComponent.maxValue ? GoapStatus.SUCCESS : GoapStatus.RUNNING;
        }
    }
}