namespace Common.Fsm.Action {
	public class TimedWaitAction : FsmAction {		
		private float waitTime;
		private readonly CountdownTimer timer;
		private readonly string finishEvent;
		
		/**
		 * Constructor
		 */
		public TimedWaitAction(FsmState owner, string timeReferenceName, string finishEvent) : base(owner) {
			if(string.IsNullOrEmpty(timeReferenceName)) {
				this.timer = new CountdownTimer(1);
			} else {
				this.timer = new CountdownTimer(1, timeReferenceName); // dummy only, we reset on Init()
			}

			this.finishEvent = finishEvent;
		}
		
		/**
		 * Initializes the action. We provide this action so that we can manage instances of this class in an object pool.
		 */
		public void Init(float waitTime) {
			this.waitTime = waitTime;
		}
		
		public override void OnEnter() {
			if(this.waitTime.TolerantLesserThanOrEquals(0)) {
				Finish();
			}

			this.timer.Reset(this.waitTime);
		}
		
		public override void OnUpdate() {
			this.timer.Update();
			
			if(this.timer.HasElapsed()) {
				Finish();
			}
		}
		
		private void Finish() {
            if (!string.IsNullOrEmpty(this.finishEvent)) {
                // Send only if there was an event specified
                GetOwner().SendEvent(this.finishEvent);
            }
		}
		
		/**
		 * Returns the time duration ratio.
		 */
		public float GetRatio() {
			return this.timer.GetRatio();
		}

        public float Elapsed() {
            return this.timer.GetPolledTime();
        }
    }
}

