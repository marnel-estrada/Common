using System;
using Common;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Encapsulates a sequence of GoapAction instances that will be executed.
    /// </summary>
    public class GoapActionPlan {

        private ActionListSearchResult actions;
        private Action onSuccessAction; // Routines when a plan has finished
        private Action onFailAction;

        /// <summary>
        /// Constructor
        /// </summary>
        public GoapActionPlan() {
        }

        /// <summary>
        /// Clears the plan
        /// </summary>
        public void Clear() {
            this.actions = null;
        }

        /// <summary>
        /// Registers the search result to the plan
        /// </summary>
        /// <param name="searchResult"></param>
        public void Register(ActionListSearchResult searchResult) {
            this.actions = searchResult;
        }

        private int currentIndex = -1;
        private GoapAction currentAction;

        public bool Successful {
            get {
                if(this.actions == null) {
                    // Maybe it wasn't planned at all and no plan came out
                    return false;
                }

                return this.actions.Successful;
            }
        }

        private GoapResult ChangeAction(int index, GoapAgent agent) {
            this.currentIndex = index;
            this.currentAction = this.actions.GetActionAt(index);
            return this.currentAction.Start(agent);
        }

        /// <summary>
        /// Starts the execution of the plan
        /// </summary>
        /// <param name="agent"></param>
        public GoapResult ExecuteStart(GoapAgent agent) {
            Assertion.NotNull(this.actions);
            
            if(this.actions == null || this.actions.Count == 0) {
                // The current plan didn't resolve to any actions
                return GoapResult.SUCCESS;
            }

            GoapResult result = ChangeAction(0, agent);
            if (result == GoapResult.SUCCESS) {
                // Execute the rest until running or failed
                return DoOnSuccess(agent);
            }

            // This can only be running or failed
            return result;
        } 

        public GoapResult ExecuteUpdate(GoapAgent agent) {
            GoapResult actionResult = this.currentAction.Update(agent);

            switch(actionResult) {
                case GoapResult.RUNNING:
                    // Always return running if action is still running
                    return GoapResult.RUNNING;

                case GoapResult.SUCCESS:
                    if(agent.IsDebug) {
                        Debug.Log(this.currentAction.Name + ": SUCCESS!");
                    }
                    return DoOnSuccess(agent);
            }

            if (agent.IsDebug) {
                Debug.Log(this.currentAction.Name + ": FAILED!");
            }
            return GoapResult.FAILED;
        }

        private GoapResult DoOnSuccess(GoapAgent agent) {
            if(this.actions == null) {
                // No actions specified
                return GoapResult.SUCCESS;
            }

            // Check if there are more actions. If there are, return running.
            // Otherwise return success
            if(this.currentIndex + 1 < this.actions.Count) {
                for(int i = this.currentIndex + 1; i < this.actions.Count; ++i) {
                    GoapResult resultOnChange = ChangeAction(this.currentIndex + 1, agent);
                    if(resultOnChange != GoapResult.SUCCESS) {
                        // Return as soon as result is failed or running
                        // Continue switching to next actions if the result is success
                        return resultOnChange;
                    }
                }
            }

            this.onSuccessAction?.Invoke(); // Invoke if existing
            return GoapResult.SUCCESS;
        }

        public int ActionCount {
            get {
                return this.actions?.Count ?? 0;
            }
        }

        public Action OnSuccessAction {
            get {
                return this.onSuccessAction;
            }

            set {
                this.onSuccessAction = value;
            }
        }

        public Action OnFailAction {
            get {
                return onFailAction;
            }

            set {
                this.onFailAction = value;
            }
        }

        public GoapAction CurrentAction {
            get {
                return this.currentAction;
            }
        }

        /// <summary>
        /// Returns the action at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GoapAction GetActionAt(int index) {
            return this.actions.GetActionAt(index);
        }

        /// <summary>
        /// Routines when the action is finished (fail or success)
        /// </summary>
        /// <param name="agent"></param>
        public void OnFinished(GoapAgent agent) {
            if(this.actions == null) {
                // There were no actions resolved during planning
                return;
            }

            for (int i = 0; i < this.actions.Count; ++i) {
                this.actions.GetActionAt(i).OnFinished(agent);
            }
        }

        /// <summary>
        /// Runs the OnFail routines
        /// </summary>
        public void OnFail(GoapAgent agent) {
            if(this.actions == null) {
                // There were no actions resolved during planning
                return;
            }

            for(int i = this.actions.Count - 1; i >= 0; --i) {
                this.actions.GetActionAt(i).OnFail(agent);
            }

            this.onFailAction?.Invoke(); // Invoke
        }

    }
}
