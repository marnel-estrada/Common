using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class FloatMinus : GoapAtomAction {
        public NamedFloat x { get; set; }
        public NamedFloat y { get; set; }

        [EditorHint(EditorHint.SELECTION)]
        public NamedFloat result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            this.result.Value = this.x.Value - this.y.Value;
            return GoapResult.SUCCESS;
        }
    }
}