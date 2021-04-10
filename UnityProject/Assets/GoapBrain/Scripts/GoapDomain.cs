using System.Collections.Generic;

using UnityEngine;

using Common;

using System.Diagnostics;
using System.Globalization;
using Debug = UnityEngine.Debug;

namespace GoapBrain {

    /// <summary>
    /// Handles a certain set of actions that an agent can perform
    /// Also performs the planning
    /// </summary>
    public class GoapDomain : MonoBehaviour {
        [SerializeField]
        private GoapDomainData data;

        [SerializeField]
        private NamedValueLibrary variables = new NamedValueLibrary();

        private readonly PreconditionsPool preconditionsPool = new PreconditionsPool();
        private readonly SimpleList<GoapAction> actionPool = new SimpleList<GoapAction>();

        private readonly Dictionary<ConditionId, ConditionActionsSet> conditionActionSetMap = new Dictionary<ConditionId, ConditionActionsSet>();

        private ActionsSearchManager searchManager;

        // We did it this way so we don't invoke Lod2dManager.Instance in OnDestroy() which creates
        // a GameObject. Creating a GameObject is wrong when the game is already being closed.
        private GoapDomainManager manager;

#if UNITY_EDITOR      
        private static readonly Stopwatch WATCH = new Stopwatch();
#endif

        private void Awake() {
            // Interpret data if it's specified
            if(this.data != null) {
                GoapDomainInterpreter interpreter = new GoapDomainInterpreter(this);
#if UNITY_EDITOR
                // Watch load time only in editor
                WATCH.Restart();
                interpreter.Configure(this.data);
                WATCH.Stop();
                Debug.LogFormat("GOAP configure for {0} took {1} seconds", this.gameObject.name, (WATCH.ElapsedMilliseconds / 1000.0f).ToString(CultureInfo.InvariantCulture));
#else
                interpreter.Configure(this.data);
#endif
            }

            this.searchManager = new ActionsSearchManager(this);

            this.manager = GoapDomainManager.Instance;
            this.manager.Add(this);
        }

        private void OnDestroy() {
            this.manager.Remove(this);
        }

        /// <summary>
        /// Sets the variables of the domain
        /// </summary>
        /// <param name="otherVariables"></param>
        public void SetVariables(NamedValueLibrary otherVariables) {
            // Note here that we copy because the variables may come from a template source like ScriptableObject
            // We need to have our own copy
            otherVariables.CopyTo(this.variables);
        }

        /// <summary>
        /// Adds the variables from another library
        /// </summary>
        /// <param name="otherVariables"></param>
        public void AddVariables(NamedValueLibrary otherVariables) {
            this.variables.Add(otherVariables);
        }

        /// <summary>
        /// Adds a precondition resolver
        /// </summary>
        /// <param name="precondition"></param>
        /// <param name="resolver"></param>
        public void AddPreconditionResolver(string precondition, ConditionResolver resolver) {
            this.preconditionsPool.Add(precondition, resolver);
        }

        /// <summary>
        /// Adds an action
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(GoapAction action) {
            Assertion.IsTrue(!HasAction(action.Name)); // Should not contain an action with the same name before adding
            this.actionPool.Add(action);
        }

        /// <summary>
        /// Returns whether or not it already contains the specified action name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasAction(string name) {
            int count = this.actionPool.Count;
            for (int i = 0; i < count; ++i) {
                if(this.actionPool[i].Name.Equals(name)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// After all the actions and condition resolvers are added, this method is invoked so that that domain can make the preparations
        /// it needs for GOAP planning. For example, A* mapping of conditions and actions is done here.
        /// </summary>
        public void Configure() {
            // Add actions to their respective action set
            int count = this.actionPool.Count;
            for (int i = 0; i < count; ++i) {
                GoapAction action = this.actionPool[i];
                AddToActionsSet(action);
            }

            // Sort each action set
            foreach(KeyValuePair<ConditionId, ConditionActionsSet> entry in this.conditionActionSetMap) {
                entry.Value.Sort();
            }
        }

        private void AddToActionsSet(GoapAction action) {
            int effectCount = action.EffectCount;
            for (int i = 0; i < effectCount; ++i) {
                Condition effect = action.GetEffectAt(i);
                ConditionActionsSet actionSet = ResolveActionsSet(effect.Id);
                actionSet.Add(action);
            }
        }

        private ConditionActionsSet ResolveActionsSet(ConditionId conditionId) {
            if(this.conditionActionSetMap.TryGetValue(conditionId, out ConditionActionsSet actionsSet)) {
                return actionsSet;
            }

            // This means that ConditionActionsSet for the specified condition was not made yet
            // We lazy initialize
            actionsSet = new ConditionActionsSet(conditionId);
            this.conditionActionSetMap[conditionId] = actionsSet;
            return actionsSet;
        }

        private readonly ActionListSearchResult overallResult = new ActionListSearchResult();
        private readonly PlanningConditionsHandler conditionsHandler = new PlanningConditionsHandler();

        public NamedValueLibrary Variables {
            get {
                return this.variables;
            }
        }

        public GoapDomainData Data {
            get {
                return this.data;
            }
        }

        private GoapAgent agent;
        private GoapActionPlan plan;
        private bool donePlanning = true;

        private bool enqueuedForPlanning;

        /// <summary>
        /// Marks that the domain is enqueued for planning
        /// </summary>
        public void MarkEnqueuedForPlanning() {
            this.enqueuedForPlanning = true;
        }

        /// <summary>
        /// Un-marks that the domain is enqueued for planning
        /// </summary>
        public void UnmarkEnqueuedForPlanning() {
            this.enqueuedForPlanning = false;
        }

        /// <summary>
        /// Performs the GOAP planning
        /// The list of action is stored in the specified GoapActionPlan
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="plan"></param>
        public void Plan(GoapAgent agent, GoapActionPlan plan) {
            if (!this.donePlanning) {
                // Not done with previous plan yet
                return;
            }

            Assertion.IsTrue(agent.GoalCount > 0); // There are no goals
            Assertion.IsTrue(this.conditionActionSetMap.Count > 0, "There are no condition-actions mapping. Maybe you forgot to invoke Configure().");

            this.agent = agent;
            this.plan = plan;
            ClearPlanning();

            this.donePlanning = false;

            if (agent.IsDebug) {
                Debug.Log("Start Plan");
            }

            // Starts the planning
            this.searchManager.BeginPlanning();
        }

        public void ExecuteUpdate() {
            this.searchManager.Update();
        }

        /// <summary>
        /// Clears the states for planning
        /// </summary>
        public void ClearPlanning() {
            this.overallResult.Clear();
            this.conditionsHandler.Clear();
            this.preconditionsPool.Reset();
            this.plan.Clear();
        }

        /// <summary>
        /// Stops the current planning
        /// This is used when agent has waited for planning for too long
        /// </summary>
        public void StopCurrentPlanning() {
            this.donePlanning = true;
            this.searchManager.Stop();
        }

        /// <summary>
        /// Marks the planning as done
        /// </summary>
        /// <param name="actionResult"></param>
        public void MarkPlanningDone(ActionListSearchResult actionResult) {
            this.plan.Register(actionResult);
            this.donePlanning = true;
            //this.planningRoutine = null; // Empty to signal that it is done

            // Print overall result
            if (this.agent.IsDebug) {
                Debug.Log("Overall Result: " + (actionResult.Successful ? "Success" : "Fail"));
                int count = actionResult.Count;
                for (int i = 0; i < count; ++i) {
                    Debug.Log(actionResult.GetActionAt(i).Name);
                }
            }
        }

        public bool DonePlanning {
            get {
                return this.donePlanning;
            }
        }

        public bool EnqueuedForPlanning {
            get {
                return this.enqueuedForPlanning;
            }
        }

        internal PlanningConditionsHandler ConditionsHandler {
            get {
                return this.conditionsHandler;
            }
        }

        internal PreconditionsPool PreconditionsPool {
            get {
                return this.preconditionsPool;
            }
        }

        public GoapAgent Agent {
            get {
                return this.agent;
            }
        }

        private readonly Pool<ActionListSearchResult> actionListPool = new Pool<ActionListSearchResult>();

        /// <summary>
        /// Requests for an action list
        /// </summary>
        /// <returns></returns>
        public ActionListSearchResult RequestActionList() {
            ActionListSearchResult actionList = this.actionListPool.Request();
            actionList.Clear();
            return actionList;
        }

        /// <summary>
        /// Recycles an action list
        /// </summary>
        /// <param name="actionList"></param>
        public void Recycle(ActionListSearchResult actionList) {
            this.actionListPool.Recycle(actionList);
        }

        /// <summary>
        /// This is different from ResolveActionsSet as it does not lazy initialize the action set
        /// </summary>
        /// <param name="conditionId"></param>
        /// <returns></returns>
        public ConditionActionsSet GetActionsSet(ConditionId conditionId) {
            if(this.conditionActionSetMap.TryGetValue(conditionId, out ConditionActionsSet actionsSet)) {
                // This means that there actions for the specified condition
                return actionsSet;
            }

            // This means that there are no actions to alter the condition and is a dead end
            return null;
        }

        /// <summary>
        /// Applies the effects of the specified action to the planning conditions
        /// </summary>
        /// <param name="action"></param>
        /// <param name="agent"></param>
        internal void ApplyEffects(GoapAction action, GoapAgent agent) {
            int effectCount = action.EffectCount;
            for (int i = 0; i < effectCount; ++i) {
                Condition condition = action.GetEffectAt(i);

                bool previousValue;
                if (this.conditionsHandler.HasCondition(condition.Id)) {
                    // Already has the condition during planning
                    previousValue = this.conditionsHandler.GetValue(condition.Id);
                } else {
                    // Condition was not resolved yet
                    // We get from preconditions pool
                    previousValue = this.preconditionsPool.IsMet(agent, condition.Id);
                }

                this.conditionsHandler.CommitChange(condition.Id, previousValue, condition.Value);
            }
        }
    }
}
