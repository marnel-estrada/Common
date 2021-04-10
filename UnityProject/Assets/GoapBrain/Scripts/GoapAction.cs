using System;

using Common;

using UnityEngine;
using UnityEngine.Profiling;

namespace GoapBrain {
    /// <summary>
    /// Represents a GOAP action. It has preconditions, a set of atom actions, and effects.
    /// </summary>
    public sealed class GoapAction {
        private readonly string name; // Name of the action

        private readonly SimpleList<Condition> preconditions = new SimpleList<Condition>(4);
        private readonly SimpleList<GoapAtomAction> atomActions = new SimpleList<GoapAtomAction>(4);
        private readonly SimpleList<Condition> effects = new SimpleList<Condition>(1);

        private float cost; // The cost of the action

        private bool cancellable;

        // The supposed actor of the action
        // This is used in planning
        private GoapAgent planningActor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public GoapAction(string name) {
            this.name = name;
        }

        public string Name {
            get {
                return name;
            }
        }

        public float Cost {
            get {
                return cost;
            }

            set {
                this.cost = value;
            }
        }

        public GoapAgent PlanningActor {
            get {
                return planningActor;
            }

            set {
                this.planningActor = value;
            }
        }

        /// <summary>
        /// Returns whether or not the action can be executed
        /// </summary>
        /// <returns></returns>
        public bool CanExecute() {
            int count = this.atomActions.Count;
            for (int i = 0; i < count; ++i) {
                if(!this.atomActions[i].CanExecute(this.planningActor)) {
                    // One of the atom actions, can't execute
                    // The whole action can't be executed
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a precondition
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        public void AddPrecondition(string conditionName, bool value) {
            // Note that we keep conditions by ID instead of their name to conserve memory
            ConditionId conditionId = ConditionNamesDatabase.Instance.GetOrAdd(conditionName);

            // Check if it already exists
            Condition condition = GetPreconditionWithId(ref conditionId);
            if(condition != null) {
                // Same precondition already exists
                // Their values should be the same
                Assertion.IsTrue(condition.Value == value);

                // No need to add since it already exists
                // We allow this because of extensions
                // There may be extension preconditions that are the same with the action's precondition
                return;
            }

            this.preconditions.Add(new Condition(conditionId, value));
        }
        
        private Condition GetPreconditionWithId(ref ConditionId id) {
            for (int i = 0; i < this.preconditions.Count; ++i) {
                if (this.preconditions[i].Id == id) {
                    return this.preconditions[i];
                }
            }

            // No precondition with the specified name
            return null;
        }

        public int PreconditionCount {
            get {
                return this.preconditions.Count;
            }
        }

        /// <summary>
        /// Returns the precondition at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Condition GetPreconditionAt(int index) {
            return this.preconditions[index];
        }

        /// <summary>
        /// Adds an atom action
        /// </summary>
        /// <param name="atomAction"></param>
        public void AddAtomAction(GoapAtomAction atomAction) {
            this.atomActions.Add(atomAction);
        }

        /// <summary>
        /// Adds an effect
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        public void AddEffect(string conditionName, bool value) {
            // Note that we keep conditions by ID instead of their name to conserve memory
            ConditionId conditionId = ConditionNamesDatabase.Instance.GetOrAdd(conditionName);

            Assertion.IsTrue(!HasEffect(ref conditionId)); // Should not contain the specified effect yet
            this.effects.Add(new Condition(conditionId, value));
        }
        
        private bool HasEffect(ref ConditionId conditionId) {
            for (int i = 0; i < this.effects.Count; ++i) {
                if (this.effects[i].Id == conditionId) {
                    return true;
                }
            }

            return false;
        }

        public int EffectCount {
            get {
                return this.effects.Count;
            }
        }

        public bool Cancellable {
            get {
                return cancellable;
            }

            set {
                cancellable = value;
            }
        }

        /// <summary>
        /// Returns the effect at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Condition GetEffectAt(int index) {
            return this.effects[index];
        }

        private int currentActionIndex = -1;
        private GoapAtomAction currentAction;

        /// <summary>
        /// Routines when the action is about to execute
        /// </summary>
        public GoapResult Start(GoapAgent agent) {
            Assertion.IsTrue(this.atomActions.Count > 0, agent.gameObject);

            // Start with the first action
            GoapResult result = ChangeCurrentAction(0, agent);
            if(result == GoapResult.SUCCESS) {
                // The first action was already done
                // Do the rest until running or failed
                if (agent.IsDebug) {
                    Debug.Log(this.currentAction.GetType().Name + ": SUCCESS!");
                }
                return DoOnSuccess(agent);
            }

            // This can only be running or failed
            if (agent.IsDebug && result == GoapResult.FAILED) {
                Debug.Log(this.currentAction.GetType().Name + ": FAILED!");
            }
            return result;
        }

        /// <summary>
        /// Executes the atom actions in sequence
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public GoapResult Update(GoapAgent agent) {
            Profiler.BeginSample(this.currentAction.GetType().Name);
            GoapResult atomResult = this.currentAction.Update(agent);
            Profiler.EndSample();

            switch(atomResult) {
                case GoapResult.RUNNING:
                    // Always return running if atom is still running
                    return GoapResult.RUNNING;

                case GoapResult.SUCCESS:
                    if (agent.IsDebug) {
                        Debug.Log(this.currentAction.GetType().Name + ": SUCCESS!");
                    }
                    return DoOnSuccess(agent);
                
                case GoapResult.FAILED:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Otherwise, return failed
            if (agent.IsDebug) {
                Debug.Log(this.currentAction.GetType().Name + ": FAILED!");
            }
            return GoapResult.FAILED;
        }

        private GoapResult DoOnSuccess(GoapAgent agent) {
            // We check if there are more actions. If there are, return running. Otherwise, return success.
            if(this.currentActionIndex + 1 < this.atomActions.Count) {
                // There are more actions. Switch to next action and return running.
                // Keep switching to the next action if the result is success
                for(int i = this.currentActionIndex + 1; i < this.atomActions.Count; ++i) {
                    GoapResult resultOnChange = ChangeCurrentAction(i, agent);

                    if(resultOnChange != GoapResult.SUCCESS) {
                        // Return if the result is fail or running
                        if(agent.IsDebug && resultOnChange == GoapResult.FAILED) {
                            Debug.Log(this.currentAction.GetType().Name + ": FAILED!");
                        }

                        return resultOnChange;
                    }
                }
            }

            return GoapResult.SUCCESS;
        }

        private GoapResult ChangeCurrentAction(int index, GoapAgent agent) {
            this.currentActionIndex = index;
            this.currentAction = this.atomActions[this.currentActionIndex];
            if (agent.IsDebug) {
                Debug.Log("Starting action " + this.currentAction.GetType().Name);
            }
            return this.currentAction.Start(agent);
        }

        /// <summary>
        /// Executes routines when the whole action is finished (fail or success)
        /// </summary>
        /// <param name="agent"></param>
        public void OnFinished(GoapAgent agent) {
            for (int i = 0; i < this.atomActions.Count; ++i) {
                this.atomActions[i].OnActionOwnerFinished(agent);
            }
        }

        /// <summary>
        /// Routines on fail
        /// </summary>
        public void OnFail(GoapAgent agent) {
            for(int i = this.atomActions.Count - 1; i >= 0; --i) {
#if !UNITY_EDITOR
                try {
#endif
                    // Recover from exceptions if on build as it causes crashes
                    this.atomActions[i].OnFail(agent);
#if !UNITY_EDITOR
                } catch(System.Exception e) {
                    Debug.LogError(e.Message);
                    Debug.LogError(e.StackTrace);
                }
#endif
            }
        }
    }
}
