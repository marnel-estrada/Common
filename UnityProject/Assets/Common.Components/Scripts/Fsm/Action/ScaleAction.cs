using UnityEngine;

namespace Common.Fsm.Action {
	/**
	 * Action for scaling on some duration.
	 */
	public class ScaleAction : FsmAction {
		
		private Transform transform; 
		private Vector3 scaleFrom; 
		private Vector3 scaleTo;
		private float duration;
		private string finishEvent;
		
		private readonly CountdownTimer timer;
		
		/**
		 * Constructor
		 */
		public ScaleAction(FsmState owner, string timeReference) : base(owner) {
			this.timer = new CountdownTimer(1, timeReference); // dummy time only here, will be set in Init()
		}
		
		/**
		 * Initializes the variables.
		 */
		public void Init(Transform transform, Vector3 scaleFrom, Vector3 scaleTo, float duration, string finishEvent) {
			this.transform = transform;
			this.scaleFrom = scaleFrom;
			this.scaleTo = scaleTo;
			this.duration = duration;
			this.finishEvent = finishEvent;
		}
		
		public override void OnEnter() {	
			if(this.duration.TolerantEquals(0)) {
				Finish();
				return;
			}
			
			if(VectorUtils.Equals(this.scaleFrom, this.scaleTo)) {
				// alphaFrom and alphaTo are already the same
				Finish();
				return;
			}

			this.transform.localScale = this.scaleFrom;
			this.timer.Reset(this.duration);
		}
		
		public override void OnUpdate() {
			this.timer.Update();
			
			if(this.timer.HasElapsed()) {
				Finish();
				return;
			}
			
			// interpolate scale
			this.transform.localScale = Vector3.Lerp(this.scaleFrom, this.scaleTo, this.timer.GetRatio());
		}
		
		private void Finish() {
			this.transform.localScale = this.scaleTo;
			
			if(!string.IsNullOrEmpty(this.finishEvent)) {
				GetOwner().SendEvent(this.finishEvent);
			}
		}
		
	}
}

