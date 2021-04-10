using Common;

namespace GoapBrain {
    [Group("GoapBrain.Sample.Collector")]
    class CarryUnclaimedObject : ComponentAction<Collector> {

        public override GoapResult Start(GoapAgent agent) {
            base.Start(agent);

            this.CachedComponent.CarryUnclaimedObject();
            return GoapResult.SUCCESS;
        }

    }
}
