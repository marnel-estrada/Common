using System;

using UnityEngine;

using Common.Fsm;
using Common.Time;

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
		
		private CountdownTimer timer;
		
		/**
		 * Constructor
		 */
		public ScaleAction(FsmState owner, string timeReference) : base(owner) {
			timer = new CountdownTimer(1, timeReference); // dummy time only here, will be set in Init()
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
			if(Comparison.TolerantEquals(duration, 0)) {
				Finish();
				return;
			}
			
			if(VectorUtils.Equals(scaleFrom, scaleTo)) {
				// alphaFrom and alphaTo are already the same
				Finish();
				return;
			}
			
			transform.localScale = scaleFrom;
			timer.Reset(this.duration);
		}
		
		public override void OnUpdate() {
			timer.Update();
			
			if(timer.HasElapsed()) {
				Finish();
				return;
			}
			
			// interpolate scale
			transform.localScale = Vector3.Lerp(scaleFrom, scaleTo, timer.GetRatio());
		}
		
		private void Finish() {
			this.transform.localScale = scaleTo;
			
			if(!string.IsNullOrEmpty(finishEvent)) {
				GetOwner().SendEvent(finishEvent);
			}
		}
		
	}
}

