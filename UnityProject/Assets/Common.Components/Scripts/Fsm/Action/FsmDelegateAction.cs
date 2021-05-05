using System;

namespace Common.Fsm {
	/**
	 * A pre created action class that uses delegates.
	 */
	public class FsmDelegateAction : FsmAction {
		
		public delegate void FsmActionRoutine(FsmState owner);
		
		private readonly FsmActionRoutine onEnterRoutine;
		private readonly FsmActionRoutine onUpdateRoutine;
		private readonly FsmActionRoutine onExitRoutine;
		
		/**
		 * Constructor with OnEnter routine.
		 */
		public FsmDelegateAction(FsmState owner, FsmActionRoutine onEnterRoutine) : this(owner, onEnterRoutine, null, null) {
		}
		
		/**
		 * Constructor with OnEnter, OnUpdate and OnExit routines.
		 */
		public FsmDelegateAction(FsmState owner, FsmActionRoutine onEnterRoutine, FsmActionRoutine onUpdateRoutine, FsmActionRoutine onExitRoutine = null) : base(owner) {
			this.onEnterRoutine = onEnterRoutine;
			this.onUpdateRoutine = onUpdateRoutine;
			this.onExitRoutine = onExitRoutine;
		}
		
		public override void OnEnter() {
			if(this.onEnterRoutine != null) {
				this.onEnterRoutine(GetOwner());
			}
		}
		
		public override void OnUpdate() {
			if(this.onUpdateRoutine != null) {
				this.onUpdateRoutine(GetOwner());
			}
		}
		
		public override void OnExit() {
			if(this.onExitRoutine != null) {
				this.onExitRoutine(GetOwner());
			}
		}

	}
}

