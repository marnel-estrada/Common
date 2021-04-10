using Common;

namespace GoapBrain {
    [Group("GoapBrain.Sample.Collector")]
    class ProcessShinyObject : ComponentAction<Collector> {

        public override GoapResult Start(GoapAgent agent) {
            base.Start(agent);

            this.CachedComponent.ProcessShinyObject();
            return GoapResult.SUCCESS;
        }

    }
}
