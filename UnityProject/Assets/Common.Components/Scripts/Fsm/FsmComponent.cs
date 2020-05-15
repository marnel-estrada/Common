using UnityEngine;

namespace Common.Fsm {
	/**
	 * An FSM wrapped in a MonoBehaviour
	 * We made this component so that we don't forget common boilerplate like Update()
	 */
	public class FsmComponent : MonoBehaviour {

		private Fsm fsm;

		[SerializeField] // for debugging only
		private string currentState; 

		/**
		 * Treat this as a contructor that should be invoked first
		 */
		public void Init(string fsmName) {
			this.fsm = new Fsm(fsmName);
		}

		/**
		 * Adds a state to the FSM.
		 */
		public FsmState AddState(string name) {
			return this.fsm.AddState(name);
		}

		/**
		 * Starts the FSM with the specified state name as the starting state.
		 */
		public void StartState(string stateName) {
			this.fsm.Start(stateName);
		}

		/**
		 * Returns the current state.
		 */
		public FsmState GetCurrentState() {
			return this.fsm.GetCurrentState();
		}

		/**
		 * Sends an event which may cause state change.
		 */
		public void SendEvent(string eventId) {
			this.fsm.SendEvent(eventId);
		}

		void Update() {
			if(this.fsm != null) {
				this.fsm.Update();

				FsmState currentState = this.fsm.GetCurrentState();
				if(currentState != null) {
					// we do this check because FSM might not have been started yet
					this.currentState = this.fsm.GetCurrentState().GetName();
				}
			}
		}

	}
}
