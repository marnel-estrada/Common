using Common;

namespace GameEvent {
    [Group("GameEvent.Sample")]
    public class SampleFundsEffect : Effect {
        public NamedInt amount { get; set; }

        public override void Apply() {
            // Just a sample
        }
    }
}