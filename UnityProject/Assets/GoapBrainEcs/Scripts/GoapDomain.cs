using System.Collections.Generic;

using Common;

namespace GoapBrainEcs {
    public class GoapDomain {
        private readonly ushort id;
        
        // Keeps track of added actions to avoid duplicates
        private readonly HashSet<ushort> addedActions = new HashSet<ushort>();
        
        private readonly Dictionary<ushort, ActionSet> effectToActionsMap = new Dictionary<ushort, ActionSet>(10);
        
        // This is a map of actionId to GoapAction
        private readonly Dictionary<ushort, GoapAction> actionsMap = new Dictionary<ushort, GoapAction>(10);
        
        private readonly Dictionary<ushort, IConditionResolverComposer> conditionResolverMap = new Dictionary<ushort, IConditionResolverComposer>(10);
        
        // Mapping of action ID to its AtomActionSet
        private readonly Dictionary<ushort, AtomActionSet> actionToAtomsMap = new Dictionary<ushort, AtomActionSet>(4);

        public GoapDomain(ushort id) {
            this.id = id;
        }

        public ushort Id {
            get {
                return this.id;
            }
        }

        /// <summary>
        /// Adds an action to the domain
        /// </summary>
        /// <param name="action"></param>
        /// <param name="atomActionSet"></param>
        public void AddAction(in GoapAction action, AtomActionSet atomActionSet) {
            // Should not contain the action yet
            Assertion.Assert(!this.addedActions.Contains(action.id));
            Assertion.Assert(atomActionSet.Count > 0); // Should have action composers
            
#if UNITY_EDITOR
            // We only check in Unity Editor because this can be slow
            Assertion.Assert(!HasCircularDependency(action, action.effect));
#endif
            
            ResolveActionSet(action.effect.id).Add(action);
            this.actionsMap[action.id] = action;
            this.actionToAtomsMap[action.id] = atomActionSet;
            this.addedActions.Add(action.id);
        }

#if UNITY_EDITOR
        private bool HasCircularDependency(in GoapAction action, Condition effect) {
            ConditionList10 preconditions = action.preconditions;
            for (int i = 0; i < preconditions.Count; ++i) {
                Condition precondition = preconditions[i];
                Maybe<IReadOnlyList<GoapAction>> actions = GetActions(precondition);
                if (!actions.HasValue) {
                    // There are no actions that satisfies the current precondition
                    continue;
                }

                if (HasCircularDependency(actions.Value, effect)) {
                    return true;
                }
            }

            return false;
        }

        private bool HasCircularDependency(IReadOnlyList<GoapAction> preconditionActions, Condition effect) {
            for (int i = 0; i < preconditionActions.Count; ++i) {
                GoapAction action = preconditionActions[i];
                if (action.HasPrecondition(effect)) {
                    // This means that one of the preconditions on the actions is the effect
                    // This causes circular dependency because the action that was checked
                    // requires this current action but this current action will also required
                    // the action being checked
                    return true;
                }

                if (HasCircularDependency(action, effect)) {
                    return true;
                }
            }
            
            return false;
        }
#endif

        private ActionSet ResolveActionSet(ushort conditionId) {
            ActionSet actionSet = this.effectToActionsMap.Find(conditionId);
            if (actionSet == null) {
                // Not yet created. We create one and maintain it.
                actionSet = new ActionSet(conditionId);
                this.effectToActionsMap[conditionId] = actionSet;
            }

            return actionSet;
        }

        /// <summary>
        /// Returns the actions that result to the specified condition and value
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public Maybe<IReadOnlyList<GoapAction>> GetActions(Condition condition) {
            ActionSet actionSet = this.effectToActionsMap.Find(condition.id);
            if (actionSet == null) {
                // There are no actions for the specified condition
                return Maybe<IReadOnlyList<GoapAction>>.Nothing;
            }

            return new Maybe<IReadOnlyList<GoapAction>>(actionSet.GetActions(condition.value));
        }

        public GoapAction GetAction(ushort actionId) {
            // This throws error if there was no action with specified ID
            return this.actionsMap[actionId];
        }

        /// <summary>
        /// Adds a condition resolver
        /// </summary>
        /// <param name="conditionId"></param>
        /// <param name="composer"></param>
        public void AddResolverComposer(ushort conditionId, IConditionResolverComposer composer) {
            this.conditionResolverMap[conditionId] = composer;
        }

        /// <summary>
        /// Returns the resolver for the specified condition
        /// </summary>
        /// <param name="conditionId"></param>
        /// <returns></returns>
        public Maybe<IConditionResolverComposer> GetResolver(ushort conditionId) {
            IConditionResolverComposer composer = this.conditionResolverMap.Find(conditionId);
            if (composer == null) {
                // No resolver for such condition
                return Maybe<IConditionResolverComposer>.Nothing;
            }
            
            return new Maybe<IConditionResolverComposer>(composer);
        }

        public void SortActions() {
            foreach (KeyValuePair<ushort,ActionSet> entry in this.effectToActionsMap) {
                entry.Value.Sort();
            }
        }

        public AtomActionSet GetAtomActionSet(ushort actionId) {
            return this.actionToAtomsMap[actionId];
        }
    }
}