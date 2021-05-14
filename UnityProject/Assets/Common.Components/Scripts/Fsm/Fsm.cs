using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Fsm {
	public class Fsm {
		private readonly string name;
		
		private FsmState? currentState;
		private readonly Dictionary<string, FsmState> stateMap = new Dictionary<string, FsmState>();
        
        private readonly StateActionProcessor onExitProcessor;

        private readonly bool delayTransitionToNextFrame;
        private FsmState? delayedTransitionState;
		
		/**
		 * Constructor
		 */
		public Fsm(string name, bool delayTransitionToNextFrame = false) {
			this.name = name;
            this.delayTransitionToNextFrame = delayTransitionToNextFrame;
			this.currentState = null;

            this.onExitProcessor = delegate (FsmAction action) {
                FsmState? currentStateOnInvoke = this.currentState;
                action.OnExit();
                if (this.currentState != currentStateOnInvoke || this.delayedTransitionState != null) {
                    // this means that the action's OnExit() causes the FSM to change state
                    // note that states should not change state on exit
                    throw new Exception("State cannot be changed on exit of the specified state.");
                }
            };
        }
		
		/**
		 * Returns the name of the FSM.
		 */
		public string Name {
			get {
				return this.name;
			}
		}
		
		/**
		 * Adds a state to the FSM.
		 */
		public FsmState AddState(string stateName) {
			// state names should be unique
			Assertion.IsTrue(!this.stateMap.ContainsKey(stateName), stateName);
			
			FsmState newState = new FsmState(stateName, this);
			this.stateMap[stateName] = newState;
			return newState;
		}
		
		private delegate void StateActionProcessor(FsmAction action);
		
		private void ProcessStateActions(FsmState state, StateActionProcessor actionProcessor) {
			FsmState? stateOnInvoke = this.currentState;

            SimpleList<FsmAction> actionList = state.ActionList;
            FsmAction[] actions = actionList.Buffer;
            int count = actionList.Count;
            for(int i = 0; i < count; ++i) {
                actionProcessor(actions[i]);

                if (this.currentState != stateOnInvoke) {
                    // this means that the action processing caused a state change
                    // we don't continue with the rest of the actions
                    break;
                }
            }
		}
		
		/**
		 * Starts the FSM with the specified state name as the starting state.
		 */
		public void Start(string stateName) {
            this.delayedTransitionState = null; // Reset so there's no unexpected transitions

            if (this.stateMap.TryGetValue(stateName, out FsmState state)) {
				ChangeToState(state);
            } else {
	            throw new Exception($"Can't find state named \"{stateName}\".");
            }
            
		}

        /// <summary>
        /// Stops the FSM
        /// </summary>
        public void Stop() {
            this.delayedTransitionState = null; // Reset so there's no unexpected transitions
            ChangeToState(null);
        }
		
		private void ChangeToState(FsmState? state) {
			if(this.currentState != null) {
				// if there's an active current state, we exit that first
				ExitState(this.currentState);
			}
			
			this.currentState = state;
			EnterState(this.currentState);
		}
		
		private void EnterState(FsmState? state) {
            if(state == null) {
                // Null may be specified to stop the FSM
                return;
            }

            // We inlined ProcessStateActions() here so it would be faster
            FsmState? stateOnInvoke = this.currentState;

            SimpleList<FsmAction> actionList = state.ActionList;
            FsmAction[] actions = actionList.Buffer;
            int count = actionList.Count;
            for (int i = 0; i < count; ++i) {
                actions[i].OnEnter();

                // We also check for delayedTransitionState because it means that the FSM is already scheduled for changing
                // to another state
                if (this.currentState != stateOnInvoke || this.delayedTransitionState != null) {
                    // this means that the action processing caused a state change
                    // we don't continue with the rest of the actions
                    break;
                }
            }
        }
		
		private void ExitState(FsmState state) {
			ProcessStateActions(state, this.onExitProcessor);
		}
		
		/**
		 * Updates the current state.
		 */
		public void Update() {
			if(this.currentState == null) {
				return;
			}

            // Check if there was a state that was supposed to be transitioned to
            if(this.delayTransitionToNextFrame && this.delayedTransitionState != null) {
                // We cached the transition state here and set delayedTransitionState to null
                // because ChangeToState() invokes ExitState() which does not allow transition.
                // It considers this.delayedTransitionState != null as a transition so it fails assertion.
                FsmState transitionState = this.delayedTransitionState;
                this.delayedTransitionState = null;
                ChangeToState(transitionState);
            }

            if (this.currentState == null) {
                // The current state might have changed due to delayed transition
                // (There's was a null pointer exception in line 164)
                return;
            }

            // We inlined the code from ProcessStateActions() here so that Update() is a little bit faster
            FsmState statePriorToUpdate = this.currentState;

            SimpleList<FsmAction> actionList = this.currentState.ActionList;
            FsmAction[] actions = actionList.Buffer;
            int count = actionList.Count;
            for (int i = 0; i < count; ++i) {
                actions[i].OnUpdate();

                // We also check for delayedTransitionState because it means that the FSM is already scheduled for changing
                // to another state
                if (this.currentState != statePriorToUpdate || this.delayedTransitionState != null) {
                    // this means that the action processing caused a state change
                    // we don't continue with the rest of the actions
                    break;
                }
            }
		}
		
		/**
		 * Returns the current state.
		 */
		public FsmState? GetCurrentState() {
			return this.currentState;
		}

		private IOptionMatcher<FsmState>? changeStateMatcher;
		
		/**
		 * Sends an event which may cause state change.
		 */
		public void SendEvent(string eventId) {
			Assertion.IsTrue(!string.IsNullOrEmpty(eventId), "The specified eventId can't be empty.");
			
			if(this.currentState == null) {
#if UNITY_EDITOR
                Debug.Log($"Fsm {this.name} does not have a current state. Check if it was started.");
#endif
                return;
			}

			// We use a non struct matcher here so it's faster
			this.changeStateMatcher ??= new DelegateOptionMatcher<FsmState>(delegate(FsmState state) {
				if (this.delayTransitionToNextFrame) {
					// Do transition to next frame
					this.delayedTransitionState = state;
				} else {
					// No delay. Change transition right away.
					ChangeToState(state);
				}
			}, delegate {
#if UNITY_EDITOR
				// log only in Unity Editor since it lags the game even if done in build
				Debug.Log($"The current state {this.currentState?.GetName()} has no transition for event {eventId}.");
#endif
			});
			
			Option<FsmState> transitionState = this.currentState.GetTransition(eventId);
			transitionState.Match(this.changeStateMatcher);
		}
	}
}
