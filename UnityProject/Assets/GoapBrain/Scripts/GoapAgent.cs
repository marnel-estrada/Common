using System;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Fsm;
using Common.Fsm.Action;

namespace GoapBrain {
    /// <summary>
    /// An agent with GOAP AI
    /// 
    /// Usage:
    /// Prepare a GoapDomain (sets of actions) and add to a GameObject
    /// Add a GoapAgent component
    /// 
    /// Prepare agent goals
    /// agent.ClearGoals();
    /// agent.AddGoal("TasksEmpty", true);
    /// 
    /// Invoke Replan() to execute a set of actions that satisfies the goals
    /// 
    /// That's it. Watch your agent do its thing.
    /// </summary>
    public sealed class GoapAgent : MonoBehaviour {
        [SerializeField]
        private GoapDomain domain;

        // Used for adding breakpoint to a specific object
        [SerializeField]
        private bool debug;

        [SerializeField]
        private string currentState;

        [SerializeField]
        private List<Condition> fallbackGoals = new List<Condition>();

        private readonly SimpleList<Condition> goals = new SimpleList<Condition>(); // The goals used for planning
        private readonly GoapActionPlan plan = new GoapActionPlan();

        private Fsm fsm;

        private readonly SimpleList<IGoapAgentObserver> observers = new SimpleList<IGoapAgentObserver>(1);

        // We cache this so we don't have to call GoapAgentUpdateManager.Instance on application quit
        // which creates a GameObject. (Unity will report an error for this.)
        private static GoapAgentUpdateManager UPDATE_MANAGER;

        private void Awake() {
            Assertion.NotNull(this.domain);

            if (UPDATE_MANAGER == null) {
                UPDATE_MANAGER = GoapAgentUpdateManager.Instance;
            }

            UPDATE_MANAGER.Add(this);
        }

        private void OnDestroy() {
            UPDATE_MANAGER.Remove(this);
        }

        // Initial state
        private const string NO_GOAL = "NoGoal";

        // Events
        private const string GOAL_ADDED = "GoalAdded";
        private const string REPLAN_REQUESTED = "ReplanRequested";
        private const string PLAN_SUCCESS = "PlanSuccess";
        private const string PLAN_FAILED = "PlanFailed";
        private const string EXECUTE_SUCCESS = "ExecuteSuccess";
        private const string EXECUTE_FAILED = "ExecuteFailed";
        private const string FINISHED = "Finished";
        private const string EMPTY_GOAL = "EmptyGoal";

        // Cached
        private FsmState noGoal;

        // Constants
        private const float PLANNING_WAIT_DURATION = 10;

        private void PrepareFsm() {
            this.fsm = new Fsm(this.gameObject.name + ".GoapAgent");

            // States
            this.noGoal = fsm.AddState(NO_GOAL);
            FsmState hasGoal = fsm.AddState("HasGoal");
            FsmState replan = fsm.AddState("Replan");
            FsmState waitForPlanning = fsm.AddState("WaitForPlanning");
            FsmState replanWait = fsm.AddState("ReplanWait");
            FsmState execute = fsm.AddState("Execute");
            FsmState onFail = fsm.AddState("OnFail");

            // Actions
            replan.AddAction(new FsmDelegateAction(replan, delegate(FsmState owner) {
                if(this.goals.Count <= 0) {
                    // No goals
                    owner.SendEvent(EMPTY_GOAL);
                    return;
                }

                if(this.debug) {
                    Debug.Log("Replan");
                }

                GoapAgentPlanningLimiter.Instance.Enqueue(this, this.domain, this.plan);
                NotifyPlanningStarted(); // Notify observers
            }, delegate(FsmState owner) {
                // Note that planning is run in a separate thread
                // We wait for it to be finished (on Update() of this state)
                if (!this.domain.EnqueuedForPlanning && this.domain.DonePlanning) {
                    // Note here that we also check if it's no longer enqueued which means that it was done waiting for it to be planned
                    if (this.plan.Successful) {
                        owner.SendEvent(PLAN_SUCCESS);
                    } else {
                        owner.SendEvent(PLAN_FAILED);
                    }

                    NotifyPlanningEnded(); // Notify observers
                }
            }));

            // WaitForPlanning
            {
                // We added this so that when a replan is requested while the previous plan wasn't done yet,
                // we have a state to go to to wait for that plan to finish and repeat the replan again.
                // It's much harder to cancel the current planning than to wait for it.
                waitForPlanning.AddAction(new FsmDelegateAction(waitForPlanning, delegate (FsmState owner) {
                    if (this.debug) {
                        Debug.Log("waitForPlanning");
                    }
                }, delegate (FsmState owner) {
                    // Note that this is Update()
                    if (this.domain.DonePlanning) {
                        owner.SendEvent(FINISHED);
                    }
                }, delegate(FsmState owner) {
                    // On exit, we always stop the planning
                    // This is because we add a timed wait such that the agent will replan again if the planning
                    // took so long or maybe something was wrong with it.
                    this.domain.StopCurrentPlanning();
                }));

                // We also add a timer so that the agent may replan if planning took to long
                TimedWaitAction wait = new TimedWaitAction(waitForPlanning, null, FINISHED);
                wait.Init(PLANNING_WAIT_DURATION);
                waitForPlanning.AddAction(wait);
            }

            // ReplanWait
            {
                TimedWaitAction waitAction = new TimedWaitAction(replanWait, null, FINISHED); // Always use root time reference here. That's why we specify null
                waitAction.Init(1.0f / 30.0f); // 1 frame of 30fps
                replanWait.AddAction(waitAction);
            }

            execute.AddAction(new ExecuteAction(execute, this));

            onFail.AddAction(new FsmDelegateAction(onFail, delegate(FsmState owner) {
                this.plan.OnFail(this);
                owner.SendEvent(FINISHED);
            }));

            // Transitions
            this.noGoal.AddTransition(GOAL_ADDED, hasGoal);

            hasGoal.AddTransition(REPLAN_REQUESTED, replan);

            replan.AddTransition(PLAN_SUCCESS, execute);
            replan.AddTransition(PLAN_FAILED, replanWait);
            replan.AddTransition(EMPTY_GOAL, this.noGoal);
            replan.AddTransition(REPLAN_REQUESTED, waitForPlanning);

            waitForPlanning.AddTransition(FINISHED, replan);
            waitForPlanning.AddTransition(REPLAN_REQUESTED, waitForPlanning);

            replanWait.AddTransition(FINISHED, replan);
            replanWait.AddTransition(REPLAN_REQUESTED, replanWait); // Wait again if a replan is requested

            execute.AddTransition(EXECUTE_SUCCESS, replanWait);
            execute.AddTransition(REPLAN_REQUESTED, onFail); // We fail the current plan first so that the OnFail() routines would be invoked
            execute.AddTransition(EXECUTE_FAILED, onFail);

            onFail.AddTransition(FINISHED, replanWait);

            // Auto start
            this.fsm.Start(NO_GOAL);
        }

        /// <summary>
        /// We made this inner class as optimization instead of using an FsmDelegateAction (avoid delegate overhead)
        /// </summary>
        private class ExecuteAction : FsmAction {
            private readonly GoapAgent agent;

            public ExecuteAction(FsmState owner, GoapAgent agent) : base(owner) {
                this.agent = agent;
            }

            public override void OnEnter() {
                GoapResult result = this.agent.plan.ExecuteStart(this.agent);

                // Note here that FSM will only continue if the result of start execution is running
                switch (result) {
                    case GoapResult.SUCCESS:
                        GetOwner().SendEvent(EXECUTE_SUCCESS);
                        break;

                    case GoapResult.FAILED:
                        GetOwner().SendEvent(EXECUTE_FAILED);
                        break;
                }
            }

            public override void OnUpdate() {
                // OnUpdate
                GoapResult result = this.agent.plan.ExecuteUpdate(this.agent);
                switch (result) {
                    case GoapResult.SUCCESS:
                        GetOwner().SendEvent(EXECUTE_SUCCESS);
                        break;

                    case GoapResult.FAILED:
                        GetOwner().SendEvent(EXECUTE_FAILED);
                        break;
                }
            }

            public override void OnExit() {
                // We invoked this on exit instead so it will still be invoked if Replan is requested
                // while the action is not finished
                this.agent.plan.OnFinished(this.agent);
            }
        }

        /// <summary>
        /// Clears the goals
        /// </summary>
        public void ClearGoals() {
            this.goals.Clear();
        }

        /// <summary>
        /// Adds a goal
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        public void AddGoal(string conditionName, bool value) {
            if(this.fsm == null) {
                // We prepare FSM here because it may not have been instantiated when AddGoal() was invoked
                PrepareFsm();
            }

            ConditionId id = ConditionNamesDatabase.Instance.GetOrAdd(conditionName);
            this.goals.Add(new Condition(id, value));

            if(this.fsm.GetCurrentState() == this.noGoal) {
                this.fsm.SendEvent(GOAL_ADDED);
            }
        }

        public int FallbackGoalCount {
            get {
                return this.fallbackGoals.Count;
            }
        }

        /// <summary>
        /// Returns the fallback goal at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Condition GetFallbackGoalAt(int index) {
            Condition fallback = this.fallbackGoals[index];

            // We assign the ID because fallback goals don't have ID yet when they are created
            fallback.SetId(ConditionNamesDatabase.Instance.GetOrAdd(fallback.Name));
            
            return fallback;
        }

        public int GoalCount {
            get {
                return this.goals.Count;
            }
        }

        public GoapActionPlan Plan {
            get {
                return plan;
            }
        }

        public bool IsDebug {
            get {
                return this.debug;
            }

            set {
                this.debug = value;
            }
        }

        /// <summary>
        /// Returns the goal condition at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Condition GetGoalAt(int index) {
            return this.goals[index];
        }

        /// <summary>
        /// Drops the current plan and replans another set of actions to satisfy its current goals
        /// </summary>
        public void Replan() {
            this.fsm.SendEvent(REPLAN_REQUESTED);
        }

        /// <summary>
        /// The update routine
        /// Invoked by a manager (magic Update() is slow)
        /// </summary>
        public void ExecuteUpdate() {
            if (this.fsm != null) {
                this.fsm.Update();

                FsmState current = this.fsm.GetCurrentState();
                this.currentState = current != null ? current.GetName() : "(null)";
            }

            // Update the observers
            int count = this.observers.Count;
            for(int i = 0; i < count; ++i) {
                this.observers[i].Update(this);
            }
        }

        /// <summary>
        /// Sets the action to execute when the current plan succeeds
        /// </summary>
        /// <param name="onSuccess"></param>
        public void SetActionOnSuccess(Action onSuccessAction) {
            this.plan.OnSuccessAction = onSuccessAction;
        }

        /// <summary>
        /// Sets the action to execute when the current plan fails
        /// </summary>
        /// <param name="onFailAction"></param>
        public void SetActionOnFail(Action onFailAction) {
            this.plan.OnFailAction = onFailAction;
        }

        /// <summary>
        /// Adds an observer
        /// </summary>
        /// <param name="observer"></param>
        public void AddObserver(IGoapAgentObserver observer) {
            this.observers.Add(observer);
        }

        /// <summary>
        /// Removes the specified observer
        /// </summary>
        /// <param name="observer"></param>
        public void RemoveObserver(IGoapAgentObserver observer) {
            this.observers.Remove(observer);
        }

        private void NotifyPlanningStarted() {
            for(int i = 0; i < this.observers.Count; ++i) {
                this.observers[i].OnPlanningStarted(this);
            }
        }

        private void NotifyPlanningEnded() {
            for (int i = 0; i < this.observers.Count; ++i) {
                this.observers[i].OnPlanningEnded(this);
            }
        }

        /// <summary>
        /// Stops the agent
        /// </summary>
        public void Stop() {
            this.fsm?.Start(NO_GOAL);
            this.plan.Clear();
        }

        public GoapAction CurrentAction {
            get {
                return this.plan.CurrentAction;
            }
        }
    }
}
