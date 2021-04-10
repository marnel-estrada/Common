using Common;

namespace GoapBrain {
    [Group("GoapBrain.Sample.Collector")]
    class HasUnclaimedShinyObjectResolver : ComponentConditionResolver<Collector> {

        protected override bool Resolve(GoapAgent agent) {
            base.Resolve(agent); // Does the caching

            return this.CachedComponent.HasUnclaimedShinyObject();
        }

    }
}
