using System;

namespace Common.Fsm {
	/**
	 * A pre created action class that uses delegates.
	 */
	public class FsmDelegateAction : FsmAction {
		
		public delegate void FsmActionRoutine(FsmState owner);
		
		private FsmActionRoutine onEnterRoutine;
		private FsmActionRoutine onUpdateRoutine;
		private FsmActionRoutine onExitRoutine;
		
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
			if(onEnterRoutine != null) {
				onEnterRoutine(GetOwner());
			}
		}
		
		public override void OnUpdate() {
			if(onUpdateRoutine != null) {
				onUpdateRoutine(GetOwner());
			}
		}
		
		public override void OnExit() {
			if(onExitRoutine != null) {
				onExitRoutine(GetOwner());
			}
		}

	}
}

