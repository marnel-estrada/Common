using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;

using UnityEngine;

namespace GoapBrainEcs {
    public class MultipleOnFailExecution : GoapExecutionTestTemplate {
        // Conditions
        private const ushort CONDITION = 1;
        private const ushort DO_BEHAVIOUR = 2;
            
        // Action IDs
        private const ushort ACTION = 333;

        private Entity agentEntity;
        private Entity requestEntity;

        public MultipleOnFailExecution(World world, EntityManager entityManager) : base(world, entityManager) {
        }

        protected override void PrepareDomain() {
            // Prepare a sample action
            GoapAction action = new GoapAction(ACTION, 0, new Condition(DO_BEHAVIOUR, true));
            action.AddPrecondition(new Condition(CONDITION, true));
            AtomActionSet actionSet = new AtomActionSet(new IncrementCounterComposer(), new SampleOnFailComposer(), new FailTranslationComposer());
            
            this.Domain.AddAction(action, actionSet);
            this.Domain.AddResolverComposer(CONDITION, new InstantResolverComposer(true));
        }

        protected override void PrepareAgentsAndRequests(EntityManager entityManager) {
            this.agentEntity = entityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(this.Domain.Id);
            agent.AddGoal(new Condition(DO_BEHAVIOUR, true));
            entityManager.AddComponentData(this.agentEntity, agent);
            
            entityManager.AddComponentData(this.agentEntity, new Counter()); // The component to modify
            entityManager.AddComponentData(this.agentEntity, new IntTranslation());
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(this.agentEntity, entityManager);
            
            // Create the plan request
            this.requestEntity = PlanRequest.Create(this.agentEntity, entityManager);
        }

        protected override void AddActionSystems(World world, SimpleList<ComponentSystemBase> systems) {
            systems.Add(world.GetOrCreateSystem<IncrementCounterSystem>());
            systems.Add(world.GetOrCreateSystem<SampleOnFailSystems.ActionSystem>());
            systems.Add(world.GetOrCreateSystem<FailTranslation_ActionSystem>());
        }

        protected override void AddOnFailSystems(World world, SimpleList<ComponentSystemBase> systems) {
            systems.Add(world.GetOrCreateSystem<SampleOnFailSystems.OnFailSystem>());
            systems.Add(world.GetOrCreateSystem<FailTranslation_OnFailSystem>());
        }

        protected override void DoAssertions(EntityManager entityManager) {
            PlanRequest request = entityManager.GetComponentData<PlanRequest>(this.requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = entityManager.GetBuffer<ActionEntry>(this.requestEntity);
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == ACTION);

            Counter counter = entityManager.GetComponentData<Counter>(this.agentEntity);
            Debug.Log($"counter: {counter.value}");
            Assert.IsTrue(counter.value == 0);

            IntTranslation translation = entityManager.GetComponentData<IntTranslation>(this.agentEntity);
            Debug.Log($"Translation: {translation.value}");
            Assert.IsTrue(translation.value.x == 100 && translation.value.y == 100 && translation.value.z == 100);
        }
    }
}