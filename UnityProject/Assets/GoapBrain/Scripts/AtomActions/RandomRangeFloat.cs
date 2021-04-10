using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class RandomRangeFloat : GoapAtomAction {
        public NamedFloat min { get; set; }
        public NamedFloat max { get; set; }

        [EditorHint(EditorHint.SELECTION)]
        public NamedFloat result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            this.result.Value = UnityEngine.Random.Range(this.min.Value, this.max.Value);
            return GoapResult.SUCCESS;
        }
    }
}
