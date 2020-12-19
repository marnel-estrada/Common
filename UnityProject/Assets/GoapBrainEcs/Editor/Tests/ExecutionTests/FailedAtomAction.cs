using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;

using UnityEngine;

namespace GoapBrainEcs {
    public class FailedAtomAction : GoapExecutionTestTemplate {
        // Conditions
        private const ushort CONDITION = 1;
        private const ushort DO_BEHAVIOUR = 2;
            
        // Action IDs
        private const ushort ACTION = 333;

        private Entity agentEntity;
        private Entity requestEntity;

        public const int MAX_VALUE = 5;

        public FailedAtomAction(World world, EntityManager entityManager) : base(world, entityManager) {
        }

        protected override void PrepareDomain() {
            // Prepare a sample action with multiple atom actions
            GoapAction action = new GoapAction(ACTION, 0, new Condition(DO_BEHAVIOUR, true));
            action.AddPrecondition(new Condition(CONDITION, true));
            
            AtomActionSet actionSet = new AtomActionSet();
            actionSet.Add(new IncrementCounterComposer());
            actionSet.Add(new IncrementCounterUntilComposer(MAX_VALUE));
            actionSet.Add(new SetGoapStatusComposer(GoapStatus.FAILED));
            actionSet.Add(new IncrementCounterComposer());
            
            this.Domain.AddAction(action, actionSet);
            this.Domain.AddResolverComposer(CONDITION, new InstantResolverComposer(true));
        }

        protected override void PrepareAgentsAndRequests(EntityManager entityManager) {
            this.agentEntity = entityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(this.Domain.Id);
            agent.AddGoal(new Condition(DO_BEHAVIOUR, true));
            entityManager.AddComponentData(this.agentEntity, agent);
            
            entityManager.AddComponentData(this.agentEntity, new Counter()); // The component to modify
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(this.agentEntity, entityManager);
            
            // Create the plan request
            this.requestEntity = PlanRequest.Create(this.agentEntity, entityManager);
        }

        protected override void AddActionSystems(World world, SimpleList<ComponentSystemBase> systems) {
            systems.Add(world.GetOrCreateSystem<IncrementCounterSystem>());
            systems.Add(world.GetOrCreateSystem<IncrementCounterUntilSystem>());
            systems.Add(world.GetOrCreateSystem<SetGoapStatusSystem>());
        }

        protected override void DoAssertions(EntityManager entityManager) {
            PlanRequest request = entityManager.GetComponentData<PlanRequest>(this.requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = entityManager.GetBuffer<ActionEntry>(this.requestEntity);
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == ACTION);

            Counter counter = entityManager.GetComponentData<Counter>(this.agentEntity);
            Debug.Log($"counter: {counter.value}");
            
            // The counter should only reaches MAX_VALUE since the last IncrementCounterComposer should not be
            // run because of failed action before it
            Assert.IsTrue(counter.value == MAX_VALUE); 
        }
    }
}