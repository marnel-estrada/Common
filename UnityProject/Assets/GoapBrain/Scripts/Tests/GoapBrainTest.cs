using UnityEngine;

namespace GoapBrain {
    [RequireComponent(typeof(GoapDomain))]
    [RequireComponent(typeof(GoapAgent))]
    abstract class GoapBrainTest : MonoBehaviour {

        private GoapDomain domain;
        private GoapAgent agent;

        public virtual void Awake() {
            this.domain = GetComponent<GoapDomain>();
            this.agent = GetComponent<GoapAgent>();
        }

        protected static void PrintActions(GoapActionPlan plan) {
            for (int i = 0; i < plan.ActionCount; ++i) {
                Debug.LogFormat("{0}: {1}", i, plan.GetActionAt(i).Name);
            }
        }

        protected GoapDomain Domain {
            get {
                return domain;
            }
        }

        protected GoapAgent Agent {
            get {
                return agent;
            }
        }

        /// <summary>
        /// Creates a simple action
        /// </summary>
        /// <param name="name"></param>
        /// <param name="effectName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected GoapAction CreateAction(string name, string effectName, bool value, bool logName = true) {
            GoapAction action = new GoapAction(name);
            action.AddEffect(effectName, value);

            if (logName) {
                action.AddAtomAction(new DebugLogAction(name));
            }

            this.Domain.AddAction(action);

            return action;
        }

    }
}
