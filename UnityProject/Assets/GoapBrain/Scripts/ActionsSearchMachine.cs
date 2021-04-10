using System;

using Common;
using Common.Fsm;

using UnityEngine;

namespace GoapBrain {
    internal class ActionsSearchMachine {

        // Initial state
        private const string INITIAL_CHECK = "InitialCheck";

        // Events
        private const string FAILED = "Failed";
        private const string SUCCESS = "Success";
        private const string CONTINUE = "Continue";
        private const string FINISHED = "Finished";
        private const string YES = "Yes";
        private const string NO = "No";
        private const string SKIP = "Skip";
        private readonly Fsm fsm = new Fsm("SearchActionMachine");
        private GoapAction action; // The current action while traversing actions

        private int actionCount;
        private int actionIndex; // Used for traversing actions
        private ActionListSearchResult actionSearchResult;

        private ConditionActionsSet actionsSet;
        private GoapAgent agent;

        private Condition condition;

        // This is the search result for child condition search
        private ActionListSearchResult conditionSearchResult;
        private PlanningConditionsHandler conditionsHandler;

        private GoapDomain domain;
        private FsmState doneFailed;

        // Cached states
        private FsmState doneSuccess;

        private int preconditionCount;
        private int preconditionIndex;
        private PreconditionsPool preconditionsPool;
        private ActionListSearchResult result;
        private ActionsSearchManager searchManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        public ActionsSearchMachine() {
            PrepareFsm();
        }

        public bool IsDone {
            get {
                FsmState current = this.fsm.GetCurrentState();

                return current == this.doneSuccess || current == this.doneFailed;
            }
        }

        public bool Successful {
            get {
                return this.result.Successful;
            }
        }

        private void PrepareFsm() {
            // States
            FsmState initialCheck = this.fsm.AddState(INITIAL_CHECK);
            FsmState beginActionTraversal = this.fsm.AddState("BeginActionTraversal");
            FsmState hasMoreActions = this.fsm.AddState("HasMoreActions");
            FsmState checkAction = this.fsm.AddState("CheckAction");
            FsmState beginPreconditionTraversal = this.fsm.AddState("BeginPreconditionTraversal");
            FsmState hasMorePreconditions = this.fsm.AddState("HasMorePreconditions");
            FsmState childSearch = this.fsm.AddState("ChildSearch");
            FsmState checkActionResult = this.fsm.AddState("CheckResult");
            this.doneSuccess = this.fsm.AddState("DoneSuccess");
            this.doneFailed = this.fsm.AddState("DoneFailed");

            // Actions
            initialCheck.AddAction(new FsmDelegateAction(initialCheck, delegate(FsmState owner) {
                // Check if the condition was already met when previous actions were applied
                if (this.conditionsHandler.HasCondition(this.condition.Id) &&
                    this.conditionsHandler.GetValue(this.condition.Id) == this.condition.Value) {
                    this.result.MarkAsSuccessful();
                    owner.SendEvent(SUCCESS);

                    return;
                }

                bool preconditionValue = this.preconditionsPool.IsMet(this.agent, this.condition.Id);
                if (this.agent.IsDebug) {
                    string conditionName = ConditionNamesDatabase.Instance.GetName(this.condition.Id);
                    Debug.Log(conditionName + " - " + preconditionValue);
                }

                if (preconditionValue == this.condition.Value) {
                    // Current world condition is already met
                    // No need to search for an action
                    this.result.MarkAsSuccessful();
                    owner.SendEvent(SUCCESS);

                    return;
                }

                this.actionsSet = this.domain.GetActionsSet(this.condition.Id);
                if (this.actionsSet == null) {
                    // This means that there are no actions to alter the condition. It's a dead end
                    this.result.MarkAsFailed();
                    owner.SendEvent(FAILED);

                    return;
                }

                owner.SendEvent(CONTINUE);
            }));

            beginActionTraversal.AddAction(new FsmDelegateAction(beginActionTraversal, delegate(FsmState owner) {
                this.actionCount = this.actionsSet.GetActionCount(this.condition.Value);
                this.actionIndex = -1; // We set -1 here because HasMoreActions moves this index on enter
                owner.SendEvent(FINISHED);
            }));

            hasMoreActions.AddAction(new FsmDelegateAction(hasMoreActions, delegate(FsmState owner) {
                ++this.actionIndex;
                owner.SendEvent(this.actionIndex < this.actionCount ? YES : NO);
            }));

            checkAction.AddAction(new FsmDelegateAction(checkAction, delegate(FsmState owner) {
                this.action = this.actionsSet.GetAction(this.condition.Value, this.actionIndex);

                if (this.conditionsHandler.ContainsCheckpoint(this.action)) {
                    // This means that some parent action was still being considered if it can be executed
                    // Maybe one of its precondition is met down the line with the same action
                    // This is dangerous
                    // It can cause infinite loop
                    owner.SendEvent(SKIP);

                    return;
                }

                if (!this.action.CanExecute()) {
                    // The current action can't execute for some other programmatic condition
                    // Move to next condition
                    owner.SendEvent(SKIP);

                    return;
                }

                if (this.agent.IsDebug) {
                    Debug.Log("Considering action: " + this.action.Name);
                }

                // We set a checkpoint here so we can revert it when action can't be completed
                this.conditionsHandler.AddCheckpoint(this.action);

                // Check if actions can be found on each of its preconditions
                this.actionSearchResult = this.domain.RequestActionList();

                owner.SendEvent(CONTINUE);
            }));

            beginPreconditionTraversal.AddAction(new FsmDelegateAction(beginPreconditionTraversal,
                delegate(FsmState owner) {
                    this.preconditionCount = this.action.PreconditionCount;
                    this.preconditionIndex = -1; // We start with -1 because HasMorePreconditions moves this index
                    owner.SendEvent(FINISHED);
                }));

            hasMorePreconditions.AddAction(new FsmDelegateAction(hasMorePreconditions, delegate(FsmState owner) {
                ++this.preconditionIndex;
                owner.SendEvent(this.preconditionIndex < this.preconditionCount ? YES : NO);
            }));

            childSearch.AddAction(new FsmDelegateAction(childSearch, delegate {
                // This is OnEnter()
                Condition precondition = this.action.GetPreconditionAt(this.preconditionIndex);
                this.conditionSearchResult = this.domain.RequestActionList();

                // ActionsSearchMachine for this child search
                ActionsSearchMachine childMachine = ActionsSearchManager.Request(this.domain, this.searchManager,
                    precondition, this.agent, this.conditionSearchResult);
                this.searchManager.Push(childMachine);
                childMachine.Start();
            }, delegate(FsmState owner) {
                // This is Update()
                // Note that this is only invoked when the child ActionsSearchMachine is done and is popped

                // Only add to final result if it's successful
                if (this.conditionSearchResult.Successful) {
                    this.actionSearchResult.AddAll(this.conditionSearchResult);
                    this.actionSearchResult.MarkAsSuccessful();
                    this.domain.Recycle(this.conditionSearchResult);
                    owner.SendEvent(SUCCESS);
                } else {
                    // One condition failed
                    // This means that the action can't complete
                    this.actionSearchResult.MarkAsFailed();
                    this.domain.Recycle(this.conditionSearchResult);
                    owner.SendEvent(FAILED);
                }
            }));

            checkActionResult.AddAction(new FsmDelegateAction(checkActionResult, delegate(FsmState owner) {
                if (!this.actionSearchResult.Successful) {
                    // This only means that one of the conditions failed to search for actions
                    // Continue to next action
                    try {
                        this.conditionsHandler.RemoveCheckpoint(this.action);
                        this.domain.Recycle(this.actionSearchResult);
                    } catch (Exception e) {
                        Assertion.IsTrue(false, "Can't remove checkpoint: " + this.action.Name);
                    }

                    owner.SendEvent(FAILED);

                    return;
                }

                // At this point, it means that the action can be completed given its preconditions
                // But we need to check if the original condition that we want was changed due to effects of previous actions
                if (this.conditionsHandler.HasCondition(this.condition.Id)) {
                    // This means that the condition exists in planningMap (it was already altered by another action)
                    if (this.condition.Value == this.conditionsHandler.GetValue(this.condition.Id)) {
                        // This means that we already got the condition that we want but was made by a previous action
                        // No need to add the current action
                        this.result.Clear();
                        this.result.AddAll(this.actionSearchResult);
                        this.domain.Recycle(this.actionSearchResult);
                        owner.SendEvent(SUCCESS);

                        return;
                    }
                }

                this.result.AddAll(this.actionSearchResult);
                this.result.Add(this.
                    action); // Add the current action on top of the actions that satisfies the preconditions
                this.result.MarkAsSuccessful();
                this.domain.ApplyEffects(this.action, this.agent);
                this.domain.Recycle(this.actionSearchResult);
                owner.SendEvent(SUCCESS);
            }));

            this.doneFailed.AddAction(new FsmDelegateAction(this.doneFailed, delegate {
                // No actions can be found to satisfy the condition
                this.result.MarkAsFailed();
            }));

            // Transitions
            initialCheck.AddTransition(FAILED, this.doneSuccess);
            initialCheck.AddTransition(SUCCESS, this.doneSuccess);
            initialCheck.AddTransition(CONTINUE, beginActionTraversal);

            beginActionTraversal.AddTransition(FINISHED, hasMoreActions);

            hasMoreActions.AddTransition(YES, checkAction);
            hasMoreActions.AddTransition(NO, this.doneFailed);

            checkAction.AddTransition(SKIP, hasMoreActions);
            checkAction.AddTransition(CONTINUE, beginPreconditionTraversal);

            beginPreconditionTraversal.AddTransition(FINISHED, hasMorePreconditions);

            hasMorePreconditions.AddTransition(YES, childSearch);
            hasMorePreconditions.AddTransition(NO, checkActionResult);

            childSearch.AddTransition(SUCCESS, hasMorePreconditions);
            childSearch.AddTransition(FAILED, checkActionResult);

            checkActionResult.AddTransition(FAILED, hasMoreActions);
            checkActionResult.AddTransition(SUCCESS, this.doneSuccess);
        }

        /// <summary>
        ///     Initializer
        /// </summary>
        /// <param name="conditionsHandler"></param>
        /// <param name="preconditionsPool"></param>
        public void Init(GoapDomain domain, ActionsSearchManager searchManager) {
            this.domain = domain;
            this.conditionsHandler = this.domain.ConditionsHandler;
            this.preconditionsPool = this.domain.PreconditionsPool;

            this.searchManager = searchManager;
        }

        /// <summary>
        ///     Initializer for parameters for the search
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="agent"></param>
        /// <param name="result"></param>
        public void InitParams(Condition condition, GoapAgent agent, ActionListSearchResult result) {
            this.condition = condition;
            this.agent = agent;
            this.result = result;
        }

        /// <summary>
        ///     Starts the algorithm
        /// </summary>
        public void Start() {
            this.fsm.Start(INITIAL_CHECK);
        }

        /// <summary>
        ///     Update routines
        /// </summary>
        public void Update() {
            this.fsm.Update();
        }
    }
}