using Common;

using UnityEngine;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class GetRandomHorizontalDirection : GoapAtomAction {
        [EditorHint(EditorHint.SELECTION)]
        public NamedInt result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            this.result.Value = (int)(Random.value > 0.5f ? HorizontalDirection.RIGHT : HorizontalDirection.LEFT);
            return GoapResult.SUCCESS;
        }
    }
}