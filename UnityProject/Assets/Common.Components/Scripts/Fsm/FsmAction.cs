using System;

namespace Common.Fsm {
	/**
	 * Base class for FSM actions
	 */
	public abstract class FsmAction {
		
		private readonly FsmState owner;
		
		/**
		 * Constructor
		 */
		public FsmAction(FsmState owner) {
			this.owner = owner;
		}
        
		public FsmState GetOwner() {
			return owner;
		}

		public virtual void OnEnter() {
			// may or may not be implemented by deriving class
		}

		public virtual void OnUpdate() {
			// may or may not be implemented by deriving class
		}

		public virtual void OnExit() {
			// may or may not be implemented by deriving class
		}

	}
}
