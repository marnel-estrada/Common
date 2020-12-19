using Unity.Entities;

namespace GoapBrainEcs {
    public class CheckCounterSystem : AtomActionComponentSystem<CheckCounter> {
        private ComponentDataFromEntity<Counter> allCounters;

        protected override void OnUpdate() {
            this.allCounters = GetComponentDataFromEntity<Counter>();
            base.OnUpdate();
        }

        protected override GoapStatus Start(ref AtomAction atomAction, ref CheckCounter checkCounter) {
            Counter counter = this.allCounters[atomAction.agentEntity];
            return counter.value >= checkCounter.valueToCheck ? GoapStatus.SUCCESS : GoapStatus.FAILED;
        }
    }
}