using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;

using UnityEngine;

namespace GoapBrainEcs {
    public class FirstAtomActionFailed : GoapExecutionTestTemplate {
        // Conditions
        const ushort HAS_ICING = 1;
        const ushort HAS_CHOCOLATE = 2;
        const ushort HAS_COCOA = 3;
        
        // Action IDs
        const ushort MAKE_ICING = 111;
        const ushort MAKE_CHOCOLATE = 222;
        const ushort BUY_COCOA = 333;
        
        private Entity agentEntity;
        private Entity requestEntity;

        public FirstAtomActionFailed(World world, EntityManager entityManager) : base(world, entityManager) {
        }

        protected override void PrepareDomain() {
            // Chained actions
            {
                GoapAction action = new GoapAction(MAKE_ICING, 0, new Condition(HAS_ICING, true));
                action.AddPrecondition(HAS_CHOCOLATE, true);
                this.Domain.AddAction(action, new AtomActionSet(new IncrementCounterComposer()));
                this.Domain.AddResolverComposer(HAS_CHOCOLATE, new InstantResolverComposer(false));
            }
            
            {
                GoapAction action = new GoapAction(MAKE_CHOCOLATE, 0, new Condition(HAS_CHOCOLATE, true));
                action.AddPrecondition(HAS_COCOA, true);
                this.Domain.AddAction(action, new AtomActionSet(new IncrementCounterComposer()));
                this.Domain.AddResolverComposer(HAS_COCOA, new InstantResolverComposer(false));
            }
            
            {
                GoapAction action = new GoapAction(BUY_COCOA, 10, new Condition(HAS_COCOA, true));
                AtomActionSet actionSet = new AtomActionSet();
                actionSet.Add(new IncrementCounterComposer());
                actionSet.Add(new SetGoapStatusComposer(GoapStatus.FAILED)); // Fail at this action
                this.Domain.AddAction(action, actionSet);
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
            Assert.IsTrue(actions.Length == 3);
            Assert.IsTrue(actions[0].actionId == BUY_COCOA);
            Assert.IsTrue(actions[1].actionId == MAKE_CHOCOLATE);
            Assert.IsTrue(actions[2].actionId == MAKE_ICING);

            Counter counter = entityManager.GetComponentData<Counter>(this.agentEntity);
            Debug.Log($"counter: {counter.value}");
            Assert.IsTrue(counter.value == 1);
        }
    }
}