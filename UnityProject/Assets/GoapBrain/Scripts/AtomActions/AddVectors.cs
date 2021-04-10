using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class AddVectors : GoapAtomAction {
        public NamedVector3 v1 { get; set; }
        public NamedVector3 v2 { get; set; }
        
        [EditorHint(EditorHint.SELECTION)]
        public NamedVector3 result { get; set; }

        public override GoapResult Start(GoapAgent agent) {
            this.result.Value = this.v1.Value + this.v2.Value;
            return GoapResult.SUCCESS;
        }
    }
}