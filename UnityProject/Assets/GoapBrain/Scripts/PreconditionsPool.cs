using Common;

using System.Collections.Generic;

using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Manages a pool of preconditions and resolvers
    /// </summary>
    class PreconditionsPool {
        // Note here that we keep preconditions by their ID instead of string name
        // This is to save memory
        private readonly ListDictionary<ConditionId, ConditionResolver> preconditions = new ListDictionary<ConditionId, ConditionResolver>();

        /// <summary>
        /// Constructor
        /// </summary>
        public PreconditionsPool() {
        }

        /// <summary>
        /// Resets the preconditions
        /// </summary>
        public void Reset() {
            for(int i = 0; i < this.preconditions.Count; ++i) {
                this.preconditions.GetAt(i).Reset();
            }
        }

        /// <summary>
        /// Adds a precondition resolver
        /// </summary>
        /// <param name="precondition"></param>
        /// <param name="resolver"></param>
        public void Add(string precondition, ConditionResolver resolver) {
            ConditionId id = ConditionNamesDatabase.Instance.GetOrAdd(precondition);
            this.preconditions.Add(id, resolver);
        }

        /// <summary>
        /// Returns whether or not the specified precondition is met
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="precondition"></param>
        /// <returns></returns>
        public bool IsMet(GoapAgent agent, ConditionId precondition) {
            Option<ConditionResolver> resolver = this.preconditions.Find(precondition);
            return resolver.MatchExplicit<IsConditionMet, bool>(new IsConditionMet(agent));
        }
        
        private readonly struct IsConditionMet : IFuncOptionMatcher<ConditionResolver, bool> {
            private readonly GoapAgent agent;

            public IsConditionMet(GoapAgent agent) {
                this.agent = agent;
            }

            public bool OnSome(ConditionResolver resolver) {
                return resolver.IsMet(this.agent);
            }

            public bool OnNone() {
                return false;
            }
        }

        public int Count {
            get {
                return this.preconditions.Count;
            }
        }

        /// <summary>
        /// Returns the resolver at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ConditionResolver GetAt(int index) {
            return this.preconditions.GetAt(index);
        }

        /// <summary>
        /// Prints the conditions found in the pool
        /// Used for debugging
        /// </summary>
        public void PrintPreconditions(GoapAgent agent) {
            int count = 1;
            foreach(KeyValuePair<ConditionId, ConditionResolver> entry in this.preconditions.KeyValueEntries) {
                string conditionName = ConditionNamesDatabase.Instance.GetName(entry.Key);
                Debug.Log(count + ". " + conditionName + ": " + (entry.Value.IsMet(agent)));
                ++count;
            }
        }
    }
}
