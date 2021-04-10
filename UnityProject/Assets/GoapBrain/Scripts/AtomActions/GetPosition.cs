using Common;

using UnityEngine;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class GetPosition : ComponentAction<Transform> {
        [EditorHint(EditorHint.SELECTION)]
        public NamedVector3 result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            base.Start(agent);
            this.result.Value = this.CachedComponent.position;
            return GoapResult.SUCCESS;
        }
    }
}