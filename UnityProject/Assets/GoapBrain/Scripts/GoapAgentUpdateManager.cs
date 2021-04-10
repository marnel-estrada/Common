using Common;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// We stopped using a ComponentSystem for this because any EntityManager calls would disrupt
    /// the entity arrays in ECS. Using a MonoBehaviour ensures that agent updates are called before
    /// ECS systems.
    /// </summary>
    public class GoapAgentUpdateManager : MonoBehaviour {
        private readonly SimpleList<GoapAgent> agents = new SimpleList<GoapAgent>(100);

        /// <summary>
        /// Adds an agent
        /// </summary>
        /// <param name="agent"></param>
        public void Add(GoapAgent agent) {
            this.agents.Add(agent);
        }

        /// <summary>
        /// Removes the specified agent
        /// </summary>
        /// <param name="agent"></param>
        public void Remove(GoapAgent agent) {
            this.agents.Remove(agent);
        }
        
        private void Update() {
            int length = this.agents.Count;
            for (int i = 0; i < length; ++i) {
                GoapAgent agent = this.agents[i];
                if(!agent.gameObject.activeInHierarchy) {
                    // Not active
                    continue;
                }
                
#if UNITY_EDITOR
                // We don't do exception handling in editor so we can debug it better
                agent.ExecuteUpdate();
#else
                // Try to recover if exception was thrown at runtime
                try {
                    agent.ExecuteUpdate();
                } catch(System.Exception e) {
                    // Try to recover
                    Debug.LogError(e.Message + ": " + e.StackTrace, agent.gameObject);
                    agent.Replan();
                }
#endif
            }
        }

        private static SingletonComponent<GoapAgentUpdateManager> SINGLETON;

        public static GoapAgentUpdateManager Instance {
            get {
                return SINGLETON.Instance;
            }
        }
    }
}