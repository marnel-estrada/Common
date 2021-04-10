using Common;

namespace GoapBrain {
    [Group("GoapBrain.General")]
    public class GoapTimedWait : GoapAtomAction {
        public NamedString timeReferenceName { get; set; }
        public NamedFloat duration { get; set; }

        private CountdownTimer timer;

        public override GoapResult Start(GoapAgent agent) {
            if(this.timer == null) {
                // We only instantiate here because the instance for timeReferenceName might not have been set yet
                this.timer = new CountdownTimer(1, this.timeReferenceName.Value);
            }

            this.timer.Reset(this.WaitDuration);

            return GoapResult.RUNNING;
        }

        protected virtual float WaitDuration {
            get {
                return this.duration.Value;
            }
        }

        public override GoapResult Update(GoapAgent agent) {
            this.timer.Update();

            if(this.timer.HasElapsed()) {
                // Duration has finished
                return GoapResult.SUCCESS;
            }

            return GoapResult.RUNNING;
        }
    }
}
