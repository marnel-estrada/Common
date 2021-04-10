namespace GoapBrain {
    internal readonly struct GoapAgentPlanEntry {
        private readonly GoapDomain domain;
        private readonly GoapAgent agent;
        private readonly GoapActionPlan plan;

        public GoapAgentPlanEntry(GoapDomain domain, GoapAgent agent, GoapActionPlan plan) {
            this.domain = domain;
            this.agent = agent;
            this.plan = plan;
        }

        /// <summary>
        /// Starts the planning
        /// </summary>
        public void Plan() {
            this.domain.Plan(this.agent, this.plan);
        }

        public bool Active {
            get {
                return this.agent.gameObject.activeInHierarchy;
            }
        }

        public GoapDomain Domain {
            get {
                return this.domain;
            }
        }
    }
}
