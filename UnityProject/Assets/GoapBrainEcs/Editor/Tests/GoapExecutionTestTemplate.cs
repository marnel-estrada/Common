using Common;

using Unity.Entities;

namespace GoapBrainEcs {
    public abstract class GoapExecutionTestTemplate {
        private readonly EntityManager entityManager;
        private readonly SimpleList<ComponentSystemBase> systems = new SimpleList<ComponentSystemBase>();
        private readonly World world;
        private GoapDomain domain;

        private readonly GoapPlanningSystem planningSystem;

        public GoapExecutionTestTemplate(World world, EntityManager entityManager) {
            this.world = world;
            this.planningSystem = world.GetOrCreateSystem<GoapPlanningSystem>();
            this.entityManager = entityManager;
        }

        protected GoapDomain Domain {
            get {
                return this.domain;
            }
        }

        public void Execute(int frameCount, bool includeReplan = false) {
            // Prepare the domain
            const ushort DOMAIN = 1;
            this.domain = new GoapDomain(DOMAIN);
            this.planningSystem.Add(this.domain);

            PrepareDomain();
            PrepareAgentsAndRequests(this.entityManager);

            // Prepare systems to update
            this.systems.Add(this.planningSystem);
            this.systems.Add(this.world.GetOrCreateSystem<StartConditionResolverSystem>());
            this.systems.Add(this.world.GetOrCreateSystem<InstantResolverSystem>());
            this.systems.Add(this.world.GetOrCreateSystem<EndConditionResolverSystem>());
            this.systems.Add(this.world.GetOrCreateSystem<CheckSearchActionSystem>());
            this.systems.Add(this.world.GetOrCreateSystem<ExecuteNextActionSystem>());
            this.systems.Add(this.world.GetOrCreateSystem<ExecuteNextAtomActionSystem>());

            AddActionSystems(this.world, this.systems);

            this.systems.Add(this.world.GetOrCreateSystem<CheckAtomActionExecution>());
            this.systems.Add(this.world.GetOrCreateSystem<ExecuteNextActionOnFailSystem>());
            this.systems.Add(this.world.GetOrCreateSystem<ExecuteNextAtomActionOnFailSystem>());
            
            AddOnFailSystems(this.world, this.systems);
            
            this.systems.Add(this.world.GetOrCreateSystem<CheckAtomActionOnFailExecution>());

            if (includeReplan) {
                this.systems.Add(this.world.GetOrCreateSystem<ReplanAfterFinishSystem>());
                this.systems.Add(this.world.GetOrCreateSystem<DestroyFinishedPlansSystem>());
            }

            RunSystems(frameCount);
            DoAssertions(this.entityManager);
        }
        
        private void RunSystems(int frames) {
            for (int i = 0; i < frames; ++i) {
                for (int systemIndex = 0; systemIndex < this.systems.Count; ++systemIndex) {
                    this.systems[systemIndex].Update();
                }
            }
        }

        // Actions and resolver composers are added here
        protected abstract void PrepareDomain();

        protected abstract void PrepareAgentsAndRequests(EntityManager entityManager);

        protected abstract void AddActionSystems(World world, SimpleList<ComponentSystemBase> systems);

        protected virtual void AddOnFailSystems(World world, SimpleList<ComponentSystemBase> systems) {
            // May or may not be implemented by deriving classes
        }

        protected abstract void DoAssertions(EntityManager entityManager);
    }
}