using Common;

namespace GameEvent {
    [Group("GameEvent.Sample")]
    public class AllFieldTypesRequirement : Requirement {
        public NamedString text { get; set; }
        public NamedInt integer { get; set; }
        public NamedFloat floatingNumber { get; set; }
        public NamedBool boolean { get; set; }

        public override bool IsMet() {
            return false;
        }

        public override string UnmetText {
            get {
                return string.Empty;
            }
        }
    }
}