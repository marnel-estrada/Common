using Common;

namespace GoapBrain {
    [Group("GoapBrain.Sample.Collector")]
    class ReserveUnclaimed : ComponentAction<Collector> {

        public override GoapResult Start(GoapAgent agent) {
            base.Start(agent);

            this.CachedComponent.ReserveUnclaimed();
            return GoapResult.SUCCESS;
        }

    }
}
