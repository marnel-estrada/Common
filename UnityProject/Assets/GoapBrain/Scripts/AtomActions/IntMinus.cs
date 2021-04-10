using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class IntMinus : GoapAtomAction {
        public NamedInt x { get; set; }
        public NamedInt y { get; set; }

        [EditorHint(EditorHint.SELECTION)]
        public NamedInt result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            this.result.Value = this.x.Value - this.y.Value;
            return GoapResult.SUCCESS;
        }
    }
}