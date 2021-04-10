using Common;

using UnityEngine;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class SetPosition : ComponentAction<Transform> {
        public NamedVector3 position { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            base.Start(agent);
            this.CachedComponent.position = this.position.Value;
            return GoapResult.SUCCESS;
        }
    }
}