using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextActionOnFailSystem))]
    public class SampleOnFailSystems {        
        public class ActionSystem : AtomActionComponentSystem<SampleOnFail> {
            protected override GoapStatus Start(ref AtomAction atomAction, ref SampleOnFail actionComponent) {
                return GoapStatus.FAILED;
            }
        }

        public class OnFailSystem : AtomActionOnFailComponentSystem<SampleOnFail> {
            private ComponentDataFromEntity<Counter> allCounters;

            protected override void OnUpdate() {
                this.allCounters = GetComponentDataFromEntity<Counter>();
                
                base.OnUpdate();
            }

            protected override void OnFail(ref AtomActionOnFail onFail, ref SampleOnFail sample) {
                Counter counter = this.allCounters[sample.counterEntity];
                counter.value = 0; // Reset to zero
                this.allCounters[sample.counterEntity] = counter; // Modify
            }
        }
    }
}