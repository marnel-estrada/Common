using System.Collections.Generic;

using Common;

using UnityEngine;

namespace GoapBrain {
    class GoapAgentPlanningLimiter : MonoBehaviour {
        private static GoapAgentPlanningLimiter ONLY_INSTANCE;

        public static GoapAgentPlanningLimiter Instance {
            get {
                if(ONLY_INSTANCE == null) {
                    GameObject go = new GameObject("GoapAgentPlanningLimiter");
                    go.AddComponent<DontDestroyOnLoadComponent>();
                    ONLY_INSTANCE = go.AddComponent<GoapAgentPlanningLimiter>();
                }

                return ONLY_INSTANCE;
            }
        }

        private const int ALLOWABLE_PLANNING_COUNT = 1000;

        private readonly Queue<GoapAgentPlanEntry> planRequestQueue = new Queue<GoapAgentPlanEntry>();

        /// <summary>
        /// Enqueues an agent for planning
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="domain"></param>
        /// <param name="plan"></param>
        public void Enqueue(GoapAgent agent, GoapDomain domain, GoapActionPlan plan) {
            domain.MarkEnqueuedForPlanning();

            GoapAgentPlanEntry entry = new GoapAgentPlanEntry(domain, agent, plan);
            this.planRequestQueue.Enqueue(entry);
        }

        private void Update() {
            if(this.planRequestQueue.Count > 0) {
#if !UNITY_EDITOR
                // Handle exception only if not on editor so we could see what's causing the error
                try {
#endif
                    for (int i = 0; i < ALLOWABLE_PLANNING_COUNT; ++i) {
                        if (this.planRequestQueue.Count == 0) {
                            // No more queued planning
                            break;
                        }

                        GoapAgentPlanEntry entry = this.planRequestQueue.Dequeue();
                        if (!entry.Active) {
                            // No longer active
                            continue;
                        }

                        entry.Plan();
                        entry.Domain.UnmarkEnqueuedForPlanning();
                    }
#if !UNITY_EDITOR
                } catch(System.Exception e) {
                    // This is weird. Even if we have checked that planRequestQueue.Count > 0,
                    // planRequestQueue.Dequeue() still sometimes fails
                    Debug.LogError("GoapAgentPlanningLimiter.Update(): " + e.Message);
                }
#endif
            }
        }
    }
}
