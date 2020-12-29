using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;

using UnityEngine;

namespace GoapBrainEcs {
    /// <summary>
    /// Result of resolver which is not equal to desired precondition should search for a valid action
    /// </summary>
    public class ActionShouldReplaceFailedConditionResolver : GoapExecutionTestTemplate {
        // Conditions
        const ushort HAS_ICING = 1;
        const ushort HAS_CHOCOLATE = 2;
        
        // Action IDs
        const ushort MAKE_ICING = 111;
        const ushort MAKE_CHOCOLATE = 222;
        
        private Entity agentEntity;
        private Entity requestEntity;

        public ActionShouldReplaceFailedConditionResolver(World world, EntityManager entityManager) : base(world, entityManager) {
        }

        protected override void PrepareDomain() {
            this.Domain.AddResolverComposer(HAS_CHOCOLATE, new InstantResolverComposer(false));
            
            // Chained actions
            {
                GoapAction action = new GoapAction(MAKE_ICING, 0, new Condition(HAS_ICING, true));
                action.AddPrecondition(HAS_CHOCOLATE, true);
                this.Domain.AddAction(action, new AtomActionSet(new IncrementCounterComposer()));
            }
            
            {
                GoapAction action = new GoapAction(MAKE_CHOCOLATE, 0, new Condition(HAS_CHOCOLATE, true));
                this.Domain.AddAction(action, new AtomActionSet(new IncrementCounterComposer()));
            }
        }

        protected override void PrepareAgentsAndRequests(EntityManager entityManager) {
            this.agentEntity = entityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(this.Domain.Id);
            agent.AddGoal(new Condition(HAS_ICING, true));
            entityManager.AddComponentData(this.agentEntity, agent);
            
            entityManager.AddComponentData(this.agentEntity, new Counter()); // The component to modify
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(this.agentEntity, entityManager);
            
            // Create the plan request
            this.requestEntity = PlanRequest.Create(this.agentEntity, entityManager);
        }

        protected override void AddActionSystems(World world, SimpleList<ComponentSystemBase> systems) {
            systems.Add(world.GetOrCreateSystem<IncrementCounterSystem>());
        }

        protected override void DoAssertions(EntityManager entityManager) {
            PlanRequest request = entityManager.GetComponentData<PlanRequest>(this.requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = entityManager.GetBuffer<ActionEntry>(this.requestEntity);
            Assert.IsTrue(actions.Length == 2);
            Assert.IsTrue(actions[0].actionId == MAKE_CHOCOLATE);
            Assert.IsTrue(actions[1].actionId == MAKE_ICING);

            Counter counter = entityManager.GetComponentData<Counter>(this.agentEntity);
            Debug.Log($"counter: {counter.value}");
            Assert.IsTrue(counter.value == 2);
        }
    }
}