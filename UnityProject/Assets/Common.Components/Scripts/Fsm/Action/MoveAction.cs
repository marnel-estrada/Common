//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18449
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

using UnityEngine;

using Common.Fsm;
using Common.Time;

namespace Common.Fsm.Action {
	public class MoveAction : FsmAction {
		private Transform transform; 
		private Vector3 positionFrom; 
		private Vector3 positionTo;
		private float duration;
		private string finishEvent;
		private Space space;
		
		private CountdownTimer timer;

		/**
		 * Constructor
		 */
		public MoveAction(FsmState owner) : base(owner) {
			this.timer = new CountdownTimer(1); // uses default time reference
		}

		/**
		 * Constructor with specified time reference name
		 */
		public MoveAction(FsmState owner, string timeReferenceName) : base(owner) {
			this.timer = new CountdownTimer(1, timeReferenceName);
		}

		/**
		 * Initializes the variables.
		 */
		public void Init(Transform transform, Vector3 positionFrom, Vector3 positionTo, float duration, string finishEvent, Space space = Space.World) {
			this.transform = transform;
			this.positionFrom = positionFrom;
			this.positionTo = positionTo;
			this.duration = duration;
			this.finishEvent = finishEvent;
			this.space = space;
		}

		public override void OnEnter() {
			if(duration.IsZero()) {
				Finish();
				return;
			}
			
			if(VectorUtils.Equals(this.positionFrom, this.positionTo)) {
				// positionFrom and positionTo are already the same
				Finish();
				return;
			}
			
			SetPosition(this.positionFrom);
			timer.Reset(this.duration);
		}

		public override void OnUpdate() {
			timer.Update();
			
			if(timer.HasElapsed()) {
				Finish();
				return;
			}
			
			// interpolate position
			SetPosition(Vector3.Lerp(this.positionFrom, this.positionTo, timer.GetRatio()));
		}

		private void Finish() {
			// snap to destination
			SetPosition(this.positionTo);
			
			if(!string.IsNullOrEmpty(finishEvent)) {
				GetOwner().SendEvent(finishEvent);
			}
		}

		private void SetPosition(Vector3 position) {
			switch(this.space) {
			case Space.World:
				this.transform.position = position;
				break;

			case Space.Self:
				this.transform.localPosition = position;
				break;
			}
		}
	}
}
