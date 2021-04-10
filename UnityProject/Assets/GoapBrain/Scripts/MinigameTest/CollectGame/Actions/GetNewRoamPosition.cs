using Common;

namespace GoapBrain {
    [Group("GoapBrain.Sample.Collector")]
    class GetNewRoamPosition : ComponentAction<Collector> {

        [EditorHint(EditorHint.SELECTION)]
        public NamedVector3 result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            base.Start(agent);

            this.result.Value = this.CachedComponent.GetNewRoamPosition();
            return GoapResult.SUCCESS;
        }

    }
}
