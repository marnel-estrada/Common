using System.Collections.Generic;
using UnityEngine;
using Common;

namespace GoapBrain {
    [CreateAssetMenu(menuName = "GoapBrain/GoapDomainData")]
    public class GoapDomainData : ScriptableObject {
        // Note here that this only keeps track of names
        // It doesn't care about values yet
        [SerializeField]
        private List<ConditionName> conditionNames = new();

        [SerializeField]
        private NamedValueLibrary variables = new();

        [SerializeField]
        private List<GoapActionData> actions = new();

        [SerializeField]
        private List<ConditionResolverData> conditionResolvers = new();

        [SerializeField]
        private List<GoapExtensionData> extensions = new();

        /// <summary>
        /// Adds a condition
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public void AddConditionName(string name) {
            Assertion.NotEmpty(name);
            
            if (ConditionExists(name)) {
                // We don't add conditions that already exists
                return;
            }

            ConditionName condition = new() {
                Name = name
            };

            this.conditionNames.Add(condition);
            this.conditionNames.Sort();
        }

        /// <summary>
        /// Adds the conditions in the specified list that does not exist in this domain data yet.
        /// </summary>
        /// <param name="conditions"></param>
        public void AddNonExistentConditions(List<ConditionData> conditions) {
            foreach (ConditionData conditionToAdd in conditions) {
                if (string.IsNullOrWhiteSpace(conditionToAdd.Name)) {
                    // Skip empty condition names
                    continue;
                }

                if (ConditionExists(conditionToAdd.Name)) {
                    // We don't add conditions that already exists
                    continue;
                }

                ConditionName newCondition = new() {
                    Name = conditionToAdd.Name
                };
                this.conditionNames.Add(newCondition);
            }
            
            this.conditionNames.Sort();
        }

        private bool ConditionExists(string conditionName) {
            foreach (ConditionName condition in this.conditionNames) {
                if (condition.Name.EqualsFast(conditionName)) {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Returns the ConditionName with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConditionName? GetConditionName(string name) {
            for (int i = 0; i < this.conditionNames.Count; ++i) {
                if (this.conditionNames[i].Name.Equals(name)) {
                    return this.conditionNames[i];
                }
            }

            // Client code should check for this
            return null;
        }

        public int ConditionNamesCount => this.conditionNames.Count;

        /// <summary>
        /// Returns the ConditionName at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ConditionName GetConditionNameAt(int index) {
            return this.conditionNames[index];
        }

        /// <summary>
        /// Removes the specified condition name
        /// </summary>
        /// <param name="conditionName"></param>
        public void RemoveConditionName(ConditionName conditionName) {
            this.conditionNames.Remove(conditionName);
        }

        public void SortConditionNames() {
            this.conditionNames.Sort();
        }

        /// <summary>
        /// Adds a new action
        /// </summary>
        /// <param name="actionName"></param>
        public GoapActionData AddAction(string actionName) {
            GoapActionData action = new() {
                Name = actionName
            };

            this.actions.Add(action);

            return action;
        }

        // May be used when copying from another GoapDomainData
        public void AddAction(GoapActionData action) {
            this.actions.Add(action);
        }

        /// <summary>
        /// Returns the action with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GoapActionData? GetAction(string name) {
            for (int i = 0; i < this.actions.Count; ++i) {
                if (this.actions[i].Name.Equals(name)) {
                    return this.actions[i];
                }
            }

            // Client code should check for this
            return null;
        }

        public int ActionCount => this.actions.Count;

        public List<ConditionResolverData> ConditionResolvers => this.conditionResolvers;

        public NamedValueLibrary Variables => this.variables;

        public List<GoapExtensionData> Extensions => this.extensions;

        /// <summary>
        /// Returns the action at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GoapActionData GetActionAt(int index) {
            return this.actions[index];
        }

        /// <summary>
        /// Removes the specified action
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAction(GoapActionData action) {
            this.actions.Remove(action);
        }

        /// <summary>
        /// Returns the ConditionResolverData with the specified condition name
        /// </summary>
        /// <param name="conditionName"></param>
        /// <returns></returns>
        public ConditionResolverData? GetConditionResolver(string conditionName) {
            for (int i = 0; i < this.conditionResolvers.Count; ++i) {
                ConditionResolverData data = this.conditionResolvers[i];
                if (data.ConditionName.Equals(conditionName)) {
                    return data;
                }
            }

            // Client code should check for this
            return null;
        }

        public bool TryGetConditionResolver(string conditionName, out ConditionResolverData? result) {
            for (int i = 0; i < this.conditionResolvers.Count; ++i) {
                ConditionResolverData data = this.conditionResolvers[i];
                if (!data.ConditionName.Equals(conditionName)) {
                    continue;
                }

                result = data;
                return true;
            }

            result = null;
            return false;
        }

        public void AddConditionResolver(ConditionResolverData conditionResolver) {
            // Must not exist yet
            if (TryGetConditionResolver(conditionResolver.ConditionName, out ConditionResolverData? _)) {
                // Already contains the resolver. We don't add.
                return;
            }
            
            this.conditionResolvers.Add(conditionResolver);
        }

        /// <summary>
        /// Removes the specified resolver
        /// </summary>
        /// <param name="resolver"></param>
        public void RemoveConditionResolver(ConditionResolverData resolver) {
            this.conditionResolvers.Remove(resolver);
        }

        public bool HasAction(string actionName) {
            foreach (GoapActionData action in this.actions) {
                if (action.Name.EqualsFast(actionName)) {
                    // Found an action with the same name
                    return true;
                }
            }

            return false;
        }
    }
}