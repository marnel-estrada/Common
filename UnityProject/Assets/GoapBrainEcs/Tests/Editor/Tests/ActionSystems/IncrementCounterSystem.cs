using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(ExecuteNextAtomActionSystem))]
    [UpdateBefore(typeof(ReplanAfterFinishSystem))]
    public class IncrementCounterSystem : ComponentSystem {
        private EntityQuery query;

        private ComponentDataFromEntity<Counter> allCounters;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(AtomAction), typeof(IncrementCounter));
        }

        protected override void OnUpdate() {
            this.allCounters = GetComponentDataFromEntity<Counter>();
            
            // Note here that we don't need to add a reference for IncrementCounter because it's just a filter
            // Besides, it can't be queried because it has no data (just a tag)
            this.Entities.With(this.query).ForEach(delegate(ref AtomAction action) {
                Counter counter = this.allCounters[action.agentEntity];
                counter.value += 1;
                this.allCounters[action.agentEntity] = counter; // Modify data

                // Don't forget to set that status
                action.status = GoapStatus.SUCCESS;
            });
        }
    }
}