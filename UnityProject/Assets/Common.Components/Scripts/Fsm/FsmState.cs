using System.Collections.Generic;
using Common.Utils;

namespace Common.Fsm {
	public class FsmState {
		
		private readonly string name;
		private readonly Fsm owner;
		
		private readonly Dictionary<string, FsmState> transitionMap = new Dictionary<string, FsmState>();
		private readonly SimpleList<FsmAction> actionList = new SimpleList<FsmAction>(1);

        /**
		 * Constructor
		 */
        public FsmState(string name, Fsm owner) {
			this.name = name;
			this.owner = owner;
		}

		public string GetName() {
			return this.name;
		}

		public void AddTransition(string eventId, FsmState destinationState) {
			// can't have two transitions for the same event
			Assertion.Assert(!this.transitionMap.ContainsKey(eventId), string.Format("The state {0} already contains a transition for event {1}.", this.name, eventId));
			this.transitionMap[eventId] = destinationState;
		}

		public Option<FsmState> GetTransition(string eventId) {
			return this.transitionMap.Find(eventId);
		}

		public void AddAction(FsmAction action) {
			Assertion.Assert(!this.actionList.Contains(action), "The state already contains the specified action.");
			Assertion.Assert(action.GetOwner() == this, "The owner of the action should be this state.");
			this.actionList.Add(action);
		}
        
        public SimpleList<FsmAction> ActionList {
            get {
                return this.actionList;
            }
        }

        public void SendEvent(string eventId) {
			this.owner.SendEvent(eventId);
		}

	}
}

