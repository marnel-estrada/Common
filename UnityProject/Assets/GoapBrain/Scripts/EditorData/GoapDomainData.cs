using System.Collections.Generic;

using UnityEngine;

using Common;

namespace GoapBrain {
    [CreateAssetMenu(menuName = "GoapBrain/GoapDomainData")]
    public class GoapDomainData : ScriptableObject {
        // Note here that this only keeps track of names
        // It doesn't care about values yet
        [SerializeField]
        private List<ConditionName> conditionNames = new List<ConditionName>();

        [SerializeField]
        private NamedValueLibrary variables = new NamedValueLibrary();

        [SerializeField]
        private List<GoapActionData> actions = new List<GoapActionData>();

        [SerializeField]
        private List<ConditionResolverData> conditionResolvers = new List<ConditionResolverData>();

        [SerializeField]
        private List<GoapExtensionData> extensions = new List<GoapExtensionData>();

        /// <summary>
        /// Adds a condition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ConditionName AddConditionName(string name) {
            Assertion.NotEmpty(name);

            ConditionName condition = new ConditionName();
            condition.Name = name;

            this.conditionNames.Add(condition);
            return condition;
        }

        /// <summary>
        /// Returns the ConditionName with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConditionName GetConditionName(string name) {
            for(int i = 0; i < this.conditionNames.Count; ++i) {
                if(this.conditionNames[i].Name.Equals(name)) {
                    return this.conditionNames[i];
                }
            }

            // Client code should check for this
            return null;
        }

        public int ConditionNamesCount {
            get {
                return this.conditionNames.Count;
            }
        }

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
        /// <param name="name"></param>
        public void RemoveConditionName(ConditionName conditionName) {
            this.conditionNames.Remove(conditionName);
        }

        /// <summary>
        /// Adds a new action
        /// </summary>
        /// <param name="actionName"></param>
        public GoapActionData AddAction(string actionName) {
            GoapActionData action = new GoapActionData();
            action.Name = actionName;

            this.actions.Add(action);

            return action;
        }

        /// <summary>
        /// Returns the action with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GoapActionData GetAction(string name) {
            for(int i = 0; i < this.actions.Count; ++i) {
                if(this.actions[i].Name.Equals(name)) {
                    return this.actions[i];
                }
            }

            // Client code should check for this
            return null;
        }

        public int ActionCount {
            get {
                return this.actions.Count;
            }
        }

        public List<ConditionResolverData> ConditionResolvers {
            get {
                return conditionResolvers;
            }
        }

        public NamedValueLibrary Variables {
            get {
                return variables;
            }
        }

        public List<GoapExtensionData> Extensions {
            get {
                return extensions;
            }
        }

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
        public ConditionResolverData GetConditionResolver(string conditionName) {
            for(int i = 0; i < this.conditionResolvers.Count; ++i) {
                ConditionResolverData data = this.conditionResolvers[i];
                if(data.ConditionName.Equals(conditionName)) {
                    return data;
                }
            }

            // Client code should check for this
            return null;
        }

        /// <summary>
        /// Removes the specified resolver
        /// </summary>
        /// <param name="resolver"></param>
        public void RemoveConditionResolver(ConditionResolverData resolver) {
            this.conditionResolvers.Remove(resolver);
        }
    }
}
