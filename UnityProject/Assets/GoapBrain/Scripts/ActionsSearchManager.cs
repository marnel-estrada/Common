using System.Collections.Generic;

using Common;

using UnityEngine;

using Common.Fsm;

namespace GoapBrain {
    /// <summary>
    /// Handles non coroutine GOAP planning
    /// </summary>
    class ActionsSearchManager {
        private readonly GoapDomain domain;

        private readonly Stack<ActionsSearchMachine> machineStack = new Stack<ActionsSearchMachine>();

        private readonly Fsm fsm = new Fsm("ActionsSearchManager", true);

        private readonly ActionListSearchResult overallResult = new ActionListSearchResult();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domain"></param>
        internal ActionsSearchManager(GoapDomain domain) {
            this.domain = domain;
            PrepareFsm();
        }

        /// <summary>
        /// Begins the planning
        /// </summary>
        public void BeginPlanning() {
            ClearStack();
            this.fsm.Start(BEGIN);
        }

        /// <summary>
        /// Stops the planning
        /// </summary>
        public void Stop() {
            ClearStack();
            this.fsm.Stop();
        }

        private void ClearStack() {
            while(this.machineStack.Count > 0) {
                Recycle(this.machineStack.Pop());
            }
        }

        // Initial State
        private const string BEGIN = "Begin";

        // Events
        private const string FINISHED = "Finished";
        private const string YES = "Yes";
        private const string NO = "No";
        private const string SUCCESS = "Success";
        private const string FAILED = "Failed";
        private const string CONTINUE = "Continue";
        private const string PLAN_FALLBACK = "PlanFallback";

        private int goalCount;
        private int goalIndex;

        private ActionListSearchResult subResult;

        private int fallbackGoalCount;
        private int fallbackGoalIndex;

        private void PrepareFsm() {
            // States
            FsmState begin = fsm.AddState(BEGIN);
            FsmState hasMoreGoals = fsm.AddState("HasMoreGoals");
            FsmState search = fsm.AddState("Search");
            FsmState checkResult = fsm.AddState("CheckResult");
            FsmState beginFallbackPlan = fsm.AddState("BeginFallbackPlan");
            FsmState hasMoreFallbackGoals = fsm.AddState("HasMoreFallbackGoals");
            FsmState fallbackSearch = fsm.AddState("FallbackSearch");
            FsmState fallbackFailed = fsm.AddState("FallbackFailed");
            FsmState done = fsm.AddState("Done");

            // Actions
            begin.AddAction(new FsmDelegateAction(begin, delegate(FsmState owner) {
                // Default as successful
                this.overallResult.Clear();
                this.overallResult.MarkAsSuccessful();

                if (this.domain.Agent.IsDebug) {
                    Debug.Log("RunPlan()");
                }

                this.goalCount = this.domain.Agent.GoalCount;
                this.goalIndex = -1; // Start with -1 because HasMoreGoals moves this index

                owner.SendEvent(FINISHED);
            }));

            hasMoreGoals.AddAction(new FsmDelegateAction(hasMoreGoals, delegate(FsmState owner) {
                ++this.goalIndex;
                owner.SendEvent(this.goalIndex < this.goalCount ? YES : NO);
            }));
            
            // OnEnter only
            search.AddAction(new FsmDelegateAction(search, delegate (FsmState owner) {
                // OnEnter
                Condition goal = this.domain.Agent.GetGoalAt(this.goalIndex);
                this.subResult = this.domain.RequestActionList();
                ActionsSearchMachine machine = Request(this.domain, this, goal, this.domain.Agent, this.subResult);
                Push(machine);
                machine.Start();
            }, SearchUpdate));

            checkResult.AddAction(new FsmDelegateAction(checkResult, delegate(FsmState owner) {
                // Plan for fallback goals if it exists
                if (!this.overallResult.Successful && this.domain.Agent.FallbackGoalCount > 0) {
                    owner.SendEvent(PLAN_FALLBACK);
                } else {
                    owner.SendEvent(CONTINUE);
                }
            }));

            beginFallbackPlan.AddAction(new FsmDelegateAction(beginFallbackPlan, delegate(FsmState owner) {
                if (this.domain.Agent.IsDebug) {
                    int breakpoint = 0;
                    ++breakpoint;
                }

                this.domain.ClearPlanning();

                // Default as successful
                this.overallResult.MarkAsSuccessful();

                this.fallbackGoalCount = this.domain.Agent.FallbackGoalCount;
                this.fallbackGoalIndex = -1; // Start with -1 because it is moved upon enter in HasMoreFallbackGoals

                owner.SendEvent(FINISHED);
            }));

            hasMoreFallbackGoals.AddAction(new FsmDelegateAction(hasMoreFallbackGoals, delegate(FsmState owner) {
                ++this.fallbackGoalIndex;
                owner.SendEvent(this.fallbackGoalIndex < this.fallbackGoalCount ? YES : NO);
            }));

            fallbackFailed.AddAction(new FsmDelegateAction(fallbackFailed, delegate(FsmState owner) {
                this.overallResult.MarkAsFailed();
                owner.SendEvent(FINISHED);
            }));
            
            fallbackSearch.AddAction(new FsmDelegateAction(fallbackSearch, delegate (FsmState owner) {
                // Note that this is OnEnter
                Condition fallbackGoal = this.domain.Agent.GetFallbackGoalAt(this.fallbackGoalIndex);
                
                if (this.domain.Agent.IsDebug) {
                    int breakpoint = 0;
                    ++breakpoint;

                    Debug.LogFormat("Fallback {0}: {1}/{2} - {3}", this.fallbackGoalIndex.ToString(), fallbackGoal.Name,
                        ConditionNamesDatabase.Instance.GetName(fallbackGoal.Id), fallbackGoal.Value.ToString());
                }
                
                this.subResult = this.domain.RequestActionList();
                ActionsSearchMachine machine = Request(this.domain, this, fallbackGoal, this.domain.Agent, this.subResult);
                Push(machine);
                machine.Start();
            }, SearchUpdate));

            done.AddAction(new FsmDelegateAction(done, delegate(FsmState owner) {
                this.domain.MarkPlanningDone(this.overallResult);
            }));

            // Transitions
            begin.AddTransition(FINISHED, hasMoreGoals);

            hasMoreGoals.AddTransition(YES, search);
            hasMoreGoals.AddTransition(NO, checkResult);

            search.AddTransition(SUCCESS, hasMoreGoals);
            search.AddTransition(FAILED, checkResult);

            checkResult.AddTransition(CONTINUE, done);
            checkResult.AddTransition(PLAN_FALLBACK, beginFallbackPlan);

            beginFallbackPlan.AddTransition(FINISHED, hasMoreFallbackGoals);

            hasMoreFallbackGoals.AddTransition(YES, fallbackSearch);
            hasMoreFallbackGoals.AddTransition(NO, fallbackFailed);

            fallbackSearch.AddTransition(SUCCESS, done);
            fallbackSearch.AddTransition(FAILED, hasMoreFallbackGoals);

            fallbackFailed.AddTransition(FINISHED, done);
        }

        // Update method for Search state
        // This method should only be called when machine stack is not empty
        private void SearchUpdate(FsmState owner) {
            ActionsSearchMachine topMachine = null;

            // We used to assert count > 0, but we don't want to throw exceptions so we just send FAILED instead
            if(this.machineStack.Count <= 0) {
                owner.SendEvent(FAILED);
            }

            if(this.domain.Agent.IsDebug) {
                int breakpoint = 0;
                ++breakpoint;
            }

            topMachine = this.machineStack.Peek();
            topMachine.Update();

            if(topMachine.IsDone) {
                this.machineStack.Pop();

                if(this.machineStack.Count == 0) {
                    // This means that the top machine was the last one
                    // Check if fail or success
                    if (subResult.Successful) {
                        if (this.domain.Agent.IsDebug) {
                            int breakpoint = 0;
                            ++breakpoint;
                        }

                        this.overallResult.AddAll(subResult);
                        this.overallResult.MarkAsSuccessful();
                        this.domain.Recycle(subResult);
                        owner.SendEvent(SUCCESS);
                    } else {
                        if (this.domain.Agent.IsDebug) {
                            int breakpoint = 0;
                            ++breakpoint;
                        }

                        // One goal failed
                        // Everything fails
                        this.overallResult.MarkAsFailed();
                        this.domain.Recycle(subResult);
                        owner.SendEvent(FAILED);
                    }
                }

                Recycle(topMachine);
            }
        }

        /// <summary>
        /// Pushes a ActionsSearchMachine
        /// </summary>
        /// <param name="machine"></param>
        internal void Push(ActionsSearchMachine machine) {
            this.machineStack.Push(machine);
        }

        /// <summary>
        /// Update routines
        /// </summary>
        internal void Update() {
            this.fsm.Update();
        }

        private static readonly Pool<ActionsSearchMachine> POOL = new Pool<ActionsSearchMachine>();

        /// <summary>
        /// Requests for a ActionsSearchMachine instance
        /// </summary>
        /// <returns></returns>
        public static ActionsSearchMachine Request(GoapDomain domain, ActionsSearchManager searchManager,
            Condition condition, GoapAgent agent, ActionListSearchResult result) {
            ActionsSearchMachine machine = POOL.Request();
            machine.Init(domain, searchManager);
            machine.InitParams(condition, agent, result);

            return machine;
        }

        /// <summary>
        /// Recycles a ActionsSearchMachine instance
        /// </summary>
        /// <param name="machine"></param>
        public static void Recycle(ActionsSearchMachine machine) {
            POOL.Recycle(machine);
        }
    }
}
