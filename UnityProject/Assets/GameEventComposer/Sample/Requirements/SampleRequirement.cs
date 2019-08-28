using Common;

namespace GameEvent {
    [Group("GameEvent.Sample")]
    public class SampleRequirement : Requirement {
        public NamedBool isTrue { get; set; }

        public override bool IsMet() {
            return this.isTrue.Value;
        }
    }
}