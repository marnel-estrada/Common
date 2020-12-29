using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace GoapBrainEcs {
    public class MultipleActionsWithMultipleAtomActions : GoapExecutionTestTemplate {
        // Conditions
        private const ushort HAS_POSTER = 1;
        private const ushort CLOSE_TO_WALL = 2;
        private const ushort POSTER_POSTED = 3;
            
        // Action IDs
        private const ushort MOVE_TO_WALL = 222;
        private const ushort POST_POSTER = 333;

        private Entity agentEntity;
        private Entity requestEntity;
        
        private static readonly int3 WALL_POSITION = new int3(3, 3, 3);

        public MultipleActionsWithMultipleAtomActions(World world, EntityManager entityManager) : base(world, entityManager) {
        }

        protected override void PrepareDomain() {
            // Prepare resolvers
            this.Domain.AddResolverComposer(HAS_POSTER, new InstantResolverComposer(true));
            
            // Prepare actions
            // MoveToWall
            {
                GoapAction action = new GoapAction(MOVE_TO_WALL, 0, new Condition(CLOSE_TO_WALL, true));
                AtomActionSet atomSet = new AtomActionSet();
                atomSet.Add(new IncrementCounterUntilComposer(5));
                atomSet.Add(new MoveIntTranslationComposer(WALL_POSITION));
                this.Domain.AddAction(action, atomSet);
            }
            
            // PostPoster
            {
                GoapAction action = new GoapAction(POST_POSTER, 0, new Condition(POSTER_POSTED, true));
                action.AddPrecondition(HAS_POSTER, true);
                action.AddPrecondition(CLOSE_TO_WALL, true);
                
                AtomActionSet atomSet = new AtomActionSet();
                atomSet.Add(new IncrementCounterUntilComposer(10));
                atomSet.Add(new IncrementCounterComposer());
                
                this.Domain.AddAction(action, atomSet);
            }
        }

        protected override void PrepareAgentsAndRequests(EntityManager entityManager) {
            this.agentEntity = entityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(this.Domain.Id);
            agent.AddGoal(new Condition(POSTER_POSTED, true));
            entityManager.AddComponentData(this.agentEntity, agent);
            
            entityManager.AddComponentData(this.agentEntity, new Counter()); // The component to modify
            
            Random random = new Random((uint)UnityEngine.Random.Range(0, 10000));
            int3 randomValue = new int3(random.NextInt(20), random.NextInt(20), random.NextInt(20));
            Debug.Log($"IntTranslation position: {randomValue}");
            entityManager.AddComponentData(this.agentEntity, new IntTranslation() {
                value = randomValue
            });
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(this.agentEntity, entityManager);
            
            // Create the plan request
            this.requestEntity = PlanRequest.Create(this.agentEntity, entityManager);
        }

        protected override void AddActionSystems(World world, SimpleList<ComponentSystemBase> systems) {
            systems.Add(world.GetOrCreateSystem<IncrementCounterSystem>());
            systems.Add(world.GetOrCreateSystem<IncrementCounterUntilSystem>());
            systems.Add(world.GetOrCreateSystem<MoveIntTranslationSystem>());
        }

        protected override void DoAssertions(EntityManager entityManager) {
            PlanRequest request = entityManager.GetComponentData<PlanRequest>(this.requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = entityManager.GetBuffer<ActionEntry>(this.requestEntity);
            Assert.IsTrue(actions.Length == 2);
            Assert.IsTrue(actions[0].actionId == MOVE_TO_WALL);
            Assert.IsTrue(actions[1].actionId == POST_POSTER);

            Counter counter = entityManager.GetComponentData<Counter>(this.agentEntity);
            Debug.Log($"counter: {counter.value}");
            Assert.IsTrue(counter.value == 11); // 5 + 5 + 1
            
            IntTranslation translation = entityManager.GetComponentData<IntTranslation>(this.agentEntity);
            Debug.Log($"translation: {translation.value}");
            bool3 isEqual = translation.value == WALL_POSITION;
            Assert.True(isEqual.x && isEqual.y && isEqual.z);
        }
    }
}