using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class RandomRangeInt : GoapAtomAction {
        public NamedInt min { get; set; }
        public NamedInt max { get; set; }

        [EditorHint(EditorHint.SELECTION)]
        public NamedInt result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            // We add one to max because Random.Range() does not include the maximum number
            this.result.Value = UnityEngine.Random.Range(this.min.Value, this.max.Value + 1);
            return GoapResult.SUCCESS;
        }
    }
}