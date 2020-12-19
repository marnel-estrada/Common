using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace GoapBrainEcs {
    public class MoveIntTranslationTest : GoapExecutionTestTemplate {
        // Conditions
        private const ushort CONDITION = 1;
        private const ushort DO_BEHAVIOUR = 2;
            
        // Action IDs
        private const ushort ACTION = 333;

        private Entity agentEntity;
        private Entity requestEntity;
        
        private int3 target;
        
        public MoveIntTranslationTest(World world, EntityManager entityManager) : base(world, entityManager) {
        }

        protected override void PrepareDomain() {
            // Prepare a sample action
            GoapAction action = new GoapAction(ACTION, 0, new Condition(DO_BEHAVIOUR, true));
            action.AddPrecondition(new Condition(CONDITION, true));
            
            Random random = new Random((uint)UnityEngine.Random.Range(0, 10000));
            this.target = new int3(random.NextInt(20), random.NextInt(20), random.NextInt(20));
            AtomActionSet actionSet = new AtomActionSet(new MoveIntTranslationComposer(this.target));
            
            this.Domain.AddAction(action, actionSet);
            this.Domain.AddResolverComposer(CONDITION, new InstantResolverComposer(true));
        }

        protected override void PrepareAgentsAndRequests(EntityManager entityManager) {
            this.agentEntity = entityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(this.Domain.Id);
            agent.AddGoal(new Condition(DO_BEHAVIOUR, true));
            entityManager.AddComponentData(this.agentEntity, agent);
            
            Random random = new Random((uint)UnityEngine.Random.Range(0, 10000));
            int3 randomValue = new int3(random.NextInt(20), random.NextInt(20), random.NextInt(20));
            Debug.Log($"randomValue: {randomValue}");
            
            // The component to modify
            entityManager.AddComponentData(this.agentEntity, new IntTranslation() {
                value = randomValue
            });
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(this.agentEntity, entityManager);
            
            // Create the plan request
            this.requestEntity = PlanRequest.Create(this.agentEntity, entityManager);
        }

        protected override void AddActionSystems(World world, SimpleList<ComponentSystemBase> systems) {
            systems.Add(world.GetOrCreateSystem<MoveIntTranslationSystem>());
        }

        protected override void DoAssertions(EntityManager entityManager) {
            PlanRequest request = entityManager.GetComponentData<PlanRequest>(this.requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = entityManager.GetBuffer<ActionEntry>(this.requestEntity);
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == ACTION);

            IntTranslation translation = entityManager.GetComponentData<IntTranslation>(this.agentEntity);
            Debug.Log($"translation: {translation.value}");
            Debug.Log($"target: {this.target}");
            bool3 isEqual = translation.value == this.target;
            Assert.True(isEqual.x && isEqual.y && isEqual.z);
        }
    }
}