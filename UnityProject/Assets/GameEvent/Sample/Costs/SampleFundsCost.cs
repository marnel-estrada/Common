using Common;

namespace GameEvent {
    [Group("GameEvent.Sample")]
    public class SampleFundsCost : Cost {
        public override bool CanAfford {
            get {
                return true;
            }
        }
        
        public override void Pay() {
            // Just a sample
        }
    }
}