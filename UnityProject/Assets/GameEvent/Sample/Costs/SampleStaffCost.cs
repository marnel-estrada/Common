using Common;

namespace GameEvent {
    [Group("GameEvent.Sample")]
    public class SampleStaffCost : Cost {
        public override bool CanAfford {
            get {
                return true;
            }
        }
        public override void Pay() {
            // Just a sample
        }

        public override string Text {
            get;
        }
    }
}