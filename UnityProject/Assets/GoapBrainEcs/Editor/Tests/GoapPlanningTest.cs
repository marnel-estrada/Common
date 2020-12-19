using System;

using Common;

using CommonEcs;

using NUnit.Framework;

using Unity.Entities;
using Unity.Entities.Tests;

using UnityEngine;
using UnityEngine.TestTools;

namespace GoapBrainEcs {
    [TestFixture]
    [Category("GoapBrainEcs")]
    public class GoapPlanningTest : ECSTestsFixture {
        // Just an empty set so we could easily call GoapDomain.AddAction()
        private readonly AtomActionSet dummyAtomActionSet = new AtomActionSet(new DummyAtomActionComposer());
        
        [Test]
        public void InstantConditionResolutionTest() {
            // Conditions
            const ushort IS_INSTANT = 1;

            const ushort DOMAIN = 1;
            
            // Prepare the domain (domain with no actions)
            GoapDomain domain = new GoapDomain(DOMAIN);
            domain.AddResolverComposer(IS_INSTANT, new InstantResolverComposer(true));
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(IS_INSTANT, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            planningSystem.Update();
            this.World.GetOrCreateSystem<StartConditionResolverSystem>().Update();
            this.World.GetOrCreateSystem<InstantResolverSystem>().Update();
            this.World.GetOrCreateSystem<EndConditionResolverSystem>().Update();
            
            // This is the next frame already
            this.World.GetOrCreateSystem<StartConditionResolverSystem>().Update();

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);
        }

        [Test]
        public void MultipleFrameConditionResolutionTest() {
            // Conditions
            const ushort CONDITION = 1;

            const ushort DOMAIN = 1;
            
            // Prepare the domain (domain with no actions)
            GoapDomain domain = new GoapDomain(DOMAIN);
            domain.AddResolverComposer(CONDITION, new RunningResolverComposer(true));
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(CONDITION, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            RunningResolverSystem runningResolverSystem = this.World.GetOrCreateSystem<RunningResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            
            // Run the systems
            for (int i = 0; i < RunningResolverSystem.FRAMES_NEEDED; ++i) {
                planningSystem.Update();
                startConditionResolverSystem.Update();
                runningResolverSystem.Update();
                endConditionResolverSystem.Update();
            }
            
            // We call an extra because it is only at this point that the system identifies if the condition
            // resolution for the search is done
            startConditionResolverSystem.Update();

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);
        }

        [Test]
        public void SetRequestAsFailedIfThereAreNoSatisfyingActions() {
            // Conditions
            const ushort CONDITION = 1;
            const ushort DO_BEHAVIOUR = 2;

            const ushort DOMAIN = 1;
            
            // Prepare the domain (no actions)
            GoapDomain domain = new GoapDomain(DOMAIN);
            domain.AddResolverComposer(CONDITION, new InstantResolverComposer(true));
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(DO_BEHAVIOUR, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            RunningResolverSystem runningResolverSystem = this.World.GetOrCreateSystem<RunningResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            planningSystem.Update();
            startConditionResolverSystem.Update();
            runningResolverSystem.Update();
            endConditionResolverSystem.Update();
            checkSearchActionSystem.Update();
            
            startConditionResolverSystem.Update();

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.FAILED);
        }
        
        [Test]
        public void SingleActionTest() {
            // Conditions
            const ushort CONDITION = 1;
            const ushort DO_BEHAVIOUR = 2;
            
            // Action IDs
            const ushort ACTION = 123;

            const ushort DOMAIN = 1;
            
            // Prepare a sample action
            GoapAction action = new GoapAction(ACTION, 0, new Condition(DO_BEHAVIOUR, true));
            action.AddPrecondition(new Condition(CONDITION, true));
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            domain.AddAction(action, this.dummyAtomActionSet);
            domain.AddResolverComposer(CONDITION, new InstantResolverComposer(true));
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(DO_BEHAVIOUR, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            SimpleList<ComponentSystemBase> systems = new SimpleList<ComponentSystemBase>();
            systems.Add(planningSystem);
            systems.Add(this.World.GetOrCreateSystem<StartConditionResolverSystem>());
            systems.Add(this.World.GetOrCreateSystem<InstantResolverSystem>());
            systems.Add(this.World.GetOrCreateSystem<EndConditionResolverSystem>());
            systems.Add(this.World.GetOrCreateSystem<CheckSearchActionSystem>());

            const int FRAMES = 5;
            RunSystems(systems, FRAMES);

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == ACTION);
        }

        [Test]
        public void MultiplePreconditionTest() {
            // Conditions
            const ushort HAS_MATCH = 1;
            const ushort HAS_WOOD = 2;
            const ushort HAS_FIRE = 3;
            
            // Action IDs
            const ushort GET_MATCH = 111;
            const ushort GET_WOOD = 222;
            const ushort MAKE_FIRE = 333;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Prepare actions
            {
                GoapAction action = new GoapAction(GET_MATCH, 0, new Condition(HAS_MATCH, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(GET_WOOD, 0, new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(MAKE_FIRE, 0, new Condition(HAS_FIRE, true));
                action.AddPrecondition(new Condition(HAS_MATCH, true));
                action.AddPrecondition(new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_FIRE, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            Debug.Log($"Actions: {actions.Length}");
            Assert.IsTrue(actions.Length == 3);

            for (int i = 0; i < actions.Length; ++i) {
                Debug.Log($"Action {i}: {actions[i].actionId}");
            }
        }

        [Test]
        public void TwoActionCircularDependencyTest() {
            // Conditions
            const ushort CONDITION_A = 1;
            const ushort CONDITION_B = 2;
            
            // Action IDs
            const ushort ACTION_A = 111;
            const ushort ACTION_B = 222;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Prepare the actions
            GoapAction actionA = new GoapAction(ACTION_A, 0, new Condition(CONDITION_A, true));
            actionA.AddPrecondition(new Condition(CONDITION_B, true));
            domain.AddAction(actionA, this.dummyAtomActionSet);
            
            GoapAction actionB = new GoapAction(ACTION_B, 0, new Condition(CONDITION_B, true));
            actionB.AddPrecondition(new Condition(CONDITION_A, true));
            
            LogAssert.Expect(LogType.Error, Assertion.DEFAULT_MESSAGE);
            Assert.Throws(typeof(Exception), delegate {
                // This should cause an exceptions as it is a circular dependency
                domain.AddAction(actionB, this.dummyAtomActionSet);
            });   
        }

        [Test]
        public void MultipleActionCircularDependencyTest() {
            LogAssert.Expect(LogType.Error, Assertion.DEFAULT_MESSAGE);
            Assert.Throws(typeof(Exception), delegate {
                // This should cause an exceptions as it is a circular dependency
                MultipleActionsCircularDependency(100);
            });
        } 

        private void MultipleActionsCircularDependency(int actionCount) {
            ushort[] conditionIds = new ushort[actionCount];
            ushort[] actionIds = new ushort[actionCount];
            for (ushort i = 0; i < actionCount; ++i) {
                conditionIds[i] = i;
                actionIds[i] = i;
            }
            
            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Prepare the actions
            for (ushort i = 0; i < actionCount; ++i) {
                GoapAction action = new GoapAction(actionIds[i], 0, new Condition(conditionIds[i], true));

                if (i + 1 < actionCount) {
                    // Not yet the last item
                    action.AddPrecondition(conditionIds[i + 1], true);
                } else {
                    // It's the last item. Use the first condition id.
                    action.AddPrecondition(conditionIds[0], true);
                }
                
                domain.AddAction(action, this.dummyAtomActionSet);
            }
        }

        [Test]
        public void ResolveByConditionResolutionAndActionTest() {
            // Conditions
            const ushort HAS_MATCH = 1;
            const ushort HAS_WOOD = 2;
            const ushort HAS_FIRE = 3;
            
            // Action IDs
            const ushort MAKE_FIRE = 333;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Condition resolvers
            domain.AddResolverComposer(HAS_MATCH, new InstantResolverComposer(true));
            domain.AddResolverComposer(HAS_WOOD, new RunningResolverComposer(true));
            
            // Prepare actions
            {
                GoapAction action = new GoapAction(MAKE_FIRE, 0, new Condition(HAS_FIRE, true));
                action.AddPrecondition(new Condition(HAS_MATCH, true));
                action.AddPrecondition(new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_FIRE, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            InstantResolverSystem instantResolverSystem = this.World.GetOrCreateSystem<InstantResolverSystem>();
            RunningResolverSystem runningResolverSystem = this.World.GetOrCreateSystem<RunningResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                instantResolverSystem.Update();
                runningResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            Debug.Log($"Actions: {actions.Length}");
            Assert.IsTrue(actions.Length == 1);

            for (int i = 0; i < actions.Length; ++i) {
                Debug.Log($"Action {i}: {actions[i].actionId}");
            }
        }
        
        [Test]
        public void StoreActionEffectsTest() {
            // Conditions
            const ushort HAS_ICING = 1;
            const ushort HAS_CHOCOLATE = 2;
            const ushort HAS_CAKE = 3;
            
            // Action IDs
            const ushort MAKE_ICING = 111;
            const ushort GET_CHOCOLATE = 222;
            const ushort BAKE_CAKE = 333;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);

            {
                GoapAction action = new GoapAction(MAKE_ICING, 0, new Condition(HAS_ICING, true));
                action.AddPrecondition(HAS_CHOCOLATE, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(GET_CHOCOLATE, 10, new Condition(HAS_CHOCOLATE, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(BAKE_CAKE, 0, new Condition(HAS_CAKE, true));
                action.AddPrecondition(new Condition(HAS_ICING, true));
                action.AddPrecondition(new Condition(HAS_CHOCOLATE, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_CAKE, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);
            
            // There should only be 3 actions in the end
            // GetChocolate, MakeIcing, BakeCake
            // No need to repeat GetChocolate for BakeCake since it was already done while making the icing
            Assert.IsTrue(actions.Length == 3);
            
            Assert.IsTrue(actions[0].actionId == GET_CHOCOLATE);
            Assert.IsTrue(actions[1].actionId == MAKE_ICING);
            Assert.IsTrue(actions[2].actionId == BAKE_CAKE);
        }
        
        [Test]
        public void DeepStoredActionEffectsTest() {
            // Conditions
            const ushort HAS_ICING = 1;
            const ushort HAS_CHOCOLATE = 2;
            const ushort HAS_COCOA = 3;
            const ushort HAS_CAKE = 4;
            
            // Action IDs
            const ushort MAKE_ICING = 111;
            const ushort MAKE_CHOCOLATE = 222;
            const ushort BAKE_CAKE = 333;
            const ushort GET_COCOA = 444;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);

            {
                GoapAction action = new GoapAction(MAKE_ICING, 0, new Condition(HAS_ICING, true));
                action.AddPrecondition(HAS_CHOCOLATE, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(MAKE_CHOCOLATE, 10, new Condition(HAS_CHOCOLATE, true));
                action.AddPrecondition(HAS_COCOA, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }

            {
                GoapAction action = new GoapAction(GET_COCOA, 0, new Condition(HAS_COCOA, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(BAKE_CAKE, 0, new Condition(HAS_CAKE, true));
                action.AddPrecondition(HAS_ICING, true);
                action.AddPrecondition(HAS_CHOCOLATE, true);
                action.AddPrecondition(HAS_COCOA, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_CAKE, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);
            
            // There should only be 3 actions in the end
            // GetChocolate, MakeIcing, BakeCake
            // No need to repeat GetChocolate and GetCocoa for BakeCake since it was already done while making the icing
            Assert.IsTrue(actions.Length == 4);

            Assert.IsTrue(actions[0].actionId == GET_COCOA);
            Assert.IsTrue(actions[1].actionId == MAKE_CHOCOLATE);
            Assert.IsTrue(actions[2].actionId == MAKE_ICING);
            Assert.IsTrue(actions[3].actionId == BAKE_CAKE);
        }

        [Test]
        public void RevertActionEffectsTest() {
            // Conditions
            const ushort HAS_ICING = 1;
            const ushort HAS_CHOCOLATE = 2;
            const ushort HAS_COCOA = 3; // No action for this. Planning should opt for BUY_CHOCOLATE
            const ushort HAS_CAKE = 4;
            
            // Action IDs
            const ushort MAKE_ICING = 111;
            const ushort MAKE_CHOCOLATE = 222;
            const ushort BUY_CHOCOLATE = 333;
            const ushort BAKE_CAKE = 444;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);

            {
                GoapAction action = new GoapAction(MAKE_ICING, 0, new Condition(HAS_ICING, true));
                action.AddPrecondition(HAS_CHOCOLATE, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(MAKE_CHOCOLATE, 0, new Condition(HAS_CHOCOLATE, true));
                action.AddPrecondition(HAS_COCOA, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(BUY_CHOCOLATE, 10, new Condition(HAS_CHOCOLATE, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(BAKE_CAKE, 0, new Condition(HAS_CAKE, true));
                action.AddPrecondition(new Condition(HAS_ICING, true));
                action.AddPrecondition(new Condition(HAS_CHOCOLATE, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            domain.SortActions(); // Don't forget to sort actions
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_CAKE, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Debug.Log($"Request Status: {request.status}");
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);
            
            // There should only be 3 actions in the end
            // BuyChocolate, MakeIcing, BakeCake
            Assert.IsTrue(actions.Length == 3);
            
            Assert.IsTrue(actions[0].actionId == BUY_CHOCOLATE);
            Assert.IsTrue(actions[1].actionId == MAKE_ICING);
            Assert.IsTrue(actions[2].actionId == BAKE_CAKE);
        }
        
        [Test]
        public void TwoActionsToAnEffectButOneActionFailed() {
            // Conditions
            const ushort HAS_AXE = 1; // No action that will satisfy this
            const ushort HAS_WOOD = 2;
            
            // Action IDs
            const ushort CUT_WOOD = 111;
            const ushort GET_WOOD = 222;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Prepare actions
            {
                GoapAction action = new GoapAction(CUT_WOOD, 0, new Condition(HAS_WOOD, true));
                action.AddPrecondition(HAS_AXE, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(GET_WOOD, 10, new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_WOOD, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);
            
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == GET_WOOD);
        }
        
        [Test]
        public void ActionsMustBeSortedTest() {
            // Conditions
            const ushort HAS_AXE = 1; // No action that will satisfy this
            const ushort HAS_WOOD = 2;
            
            // Action IDs
            const ushort CUT_WOOD = 111;
            const ushort PICK_STICKS = 222;
            const ushort GET_WOOD = 333;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Prepare actions
            {
                GoapAction action = new GoapAction(CUT_WOOD, 0, new Condition(HAS_WOOD, true));
                action.AddPrecondition(HAS_AXE, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(PICK_STICKS, 5, new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(GET_WOOD, 10, new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            domain.SortActions();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_WOOD, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);

            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == PICK_STICKS);
        }

        [Test]
        public void DoChainedHierarchyTest() {
            DoChainedHierarchy(50);
        }

        private void DoChainedHierarchy(ushort depth) {
            const ushort DOMAIN = 1;
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            // Prepare actions
            for (ushort i = 0; i < depth; ++i) {
                GoapAction action = new GoapAction(i, 0, new Condition(i, true));

                // Add precondition only if not the deepest action
                if (i > 0) {
                    action.AddPrecondition((ushort)(i - 1), true);
                }
                
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            domain.SortActions();
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition((ushort)(depth - 1), true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            int frames = depth * 4;
            for (int i = 0; i < frames; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);
            
            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);

            for (int i = 0; i < actions.Length; ++i) {
                Assert.IsTrue(actions[i].actionId == i);
            }
        }
        
        // Conditions
        const ushort HAS_ICING = 1;
        const ushort HAS_CHOCOLATE = 2;
        const ushort HAS_COCOA = 3; // No action for this. Planning should opt for BUY_CHOCOLATE
        const ushort HAS_CAKE = 4;
        
        // Action IDs
        const ushort MAKE_ICING = 111;
        const ushort MAKE_CHOCOLATE = 222;
        const ushort BUY_CHOCOLATE = 333;
        const ushort BAKE_CAKE = 444;

        const ushort DOMAIN = 1;
        
        [Test]
        public void MultipleRequestTest() {
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);

            {
                GoapAction action = new GoapAction(MAKE_ICING, 0, new Condition(HAS_ICING, true));
                action.AddPrecondition(HAS_CHOCOLATE, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(MAKE_CHOCOLATE, 0, new Condition(HAS_CHOCOLATE, true));
                action.AddPrecondition(HAS_COCOA, true);
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(BUY_CHOCOLATE, 10, new Condition(HAS_CHOCOLATE, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            {
                GoapAction action = new GoapAction(BAKE_CAKE, 0, new Condition(HAS_CAKE, true));
                action.AddPrecondition(new Condition(HAS_ICING, true));
                action.AddPrecondition(new Condition(HAS_CHOCOLATE, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            domain.SortActions(); // Don't forget to sort actions
            planningSystem.Add(domain);
            
            // Create the requests
            Entity firstRequestEntity = CreateRequest(new Condition(HAS_CAKE, true), DOMAIN);
            Entity secondRequestEntity = CreateRequest(new Condition(HAS_ICING, true), DOMAIN);

            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            AssertFirstRequest(firstRequestEntity);
            AssertSecondRequest(secondRequestEntity);
        }

        private void AssertFirstRequest(Entity firstRequestEntity) {
            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(firstRequestEntity);
            Debug.Log($"Request Status: {request.status}");
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(firstRequestEntity);
            PrintActions(actions);

            // There should only be 3 actions in the end
            // BuyChocolate, MakeIcing, BakeCake
            Assert.IsTrue(actions.Length == 3);

            Assert.IsTrue(actions[0].actionId == BUY_CHOCOLATE);
            Assert.IsTrue(actions[1].actionId == MAKE_ICING);
            Assert.IsTrue(actions[2].actionId == BAKE_CAKE);
        }

        private void AssertSecondRequest(Entity secondRequestEntity) {
            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(secondRequestEntity);
            Debug.Log($"Request Status: {request.status}");
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(secondRequestEntity);
            PrintActions(actions);

            Assert.IsTrue(actions.Length == 2);

            Assert.IsTrue(actions[0].actionId == BUY_CHOCOLATE);
            Assert.IsTrue(actions[1].actionId == MAKE_ICING);
        }

        private Entity CreateRequest(Condition goal, ushort domainId) {
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(domainId);
            agent.AddGoal(goal);
            this.EntityManager.AddComponentData(agentEntity, agent);

            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);

            // Create the plan request
            return PlanRequest.Create(agentEntity, this.EntityManager);
        }
        
        [Test]
        public void SingleFallbackTest() {
            // Conditions
            const ushort HAS_WOOD = 1; // No action for this condition to make HAS_FIRE fail
            const ushort HAS_FIRE = 2;
            const ushort IS_RESTING = 3;
            
            // Action IDs
            const ushort MAKE_FIRE = 333;
            const ushort REST = 111;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            {
                GoapAction action = new GoapAction(MAKE_FIRE, 0, new Condition(HAS_FIRE, true));
                action.AddPrecondition(new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Fallback action
            {
                GoapAction action = new GoapAction(REST, 0, new Condition(IS_RESTING, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_FIRE, true));
            agent.AddFallbackGoal(new Condition(IS_RESTING, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);
            
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == REST);
        }
        
        [Test]
        public void MultipleFallbackTest() {
            // Conditions
            const ushort HAS_WOOD = 1; // No action for this condition to make HAS_FIRE fail
            const ushort HAS_FIRE = 2;
            
            // Fallback goals
            const ushort IS_PLAYING = 3;
            const ushort IS_PARTYING = 4;
            const ushort IS_RESTING = 5;
            
            // Action IDs
            const ushort MAKE_FIRE = 333;
            const ushort REST = 111;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            {
                GoapAction action = new GoapAction(MAKE_FIRE, 0, new Condition(HAS_FIRE, true));
                action.AddPrecondition(new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Fallback action
            {
                GoapAction action = new GoapAction(REST, 0, new Condition(IS_RESTING, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_FIRE, true));
            agent.AddFallbackGoal(new Condition(IS_PLAYING, true));
            agent.AddFallbackGoal(new Condition(IS_PARTYING, true));
            agent.AddFallbackGoal(new Condition(IS_RESTING, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.SUCCESS);

            DynamicBuffer<ActionEntry> actions = this.EntityManager.GetBuffer<ActionEntry>(requestEntity);
            PrintActions(actions);
            
            Assert.IsTrue(actions.Length == 1);
            Assert.IsTrue(actions[0].actionId == REST);
        }
        
        [Test]
        public void FallbackGoalsFailTest() {
            // Conditions
            const ushort HAS_WOOD = 1; // No action for this condition to make HAS_FIRE fail
            const ushort HAS_FIRE = 2;
            
            // Fallback goals (no actions that satisfies these)
            const ushort IS_PLAYING = 3;
            const ushort IS_PARTYING = 4;
            const ushort IS_RESTING = 5;
            
            // Action IDs
            const ushort MAKE_FIRE = 333;
            const ushort REST = 111;

            const ushort DOMAIN = 1;
            
            // Prepare the domain
            GoapDomain domain = new GoapDomain(DOMAIN);
            
            {
                GoapAction action = new GoapAction(MAKE_FIRE, 0, new Condition(HAS_FIRE, true));
                action.AddPrecondition(new Condition(HAS_WOOD, true));
                domain.AddAction(action, this.dummyAtomActionSet);
            }
            
            // Don't forget to add the domain
            GoapPlanningSystem planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
            planningSystem.Add(domain);
            
            // Prepare a GOAP entity and goals
            Entity agentEntity = this.EntityManager.CreateEntity();
            GoapAgent agent = new GoapAgent(DOMAIN);
            agent.AddGoal(new Condition(HAS_FIRE, true));
            agent.AddFallbackGoal(new Condition(IS_PLAYING, true));
            agent.AddFallbackGoal(new Condition(IS_PARTYING, true));
            agent.AddFallbackGoal(new Condition(IS_RESTING, true));
            this.EntityManager.AddComponentData(agentEntity, agent);
            
            // Note that each agent needs a conditions result map
            EcsHashMap<ushort, ByteBool>.Create(agentEntity, this.EntityManager);
            
            // Create the plan request
            Entity requestEntity = PlanRequest.Create(agentEntity, this.EntityManager);
            
            // Run the systems
            StartConditionResolverSystem startConditionResolverSystem = this.World.GetOrCreateSystem<StartConditionResolverSystem>();
            EndConditionResolverSystem endConditionResolverSystem = this.World.GetOrCreateSystem<EndConditionResolverSystem>();
            CheckSearchActionSystem checkSearchActionSystem = this.World.GetOrCreateSystem<CheckSearchActionSystem>();
            
            const int FRAMES = 20;
            for (int i = 0; i < FRAMES; ++i) {   
                planningSystem.Update();
                startConditionResolverSystem.Update();
                endConditionResolverSystem.Update();
                checkSearchActionSystem.Update();
            }

            PlanRequest request = this.EntityManager.GetComponentData<PlanRequest>(requestEntity);
            Assert.IsTrue(request.status == GoapStatus.FAILED);
        }

        private static void PrintActions(DynamicBuffer<ActionEntry> actions) {
            Debug.Log($"Actions: {actions.Length}");
            for (int i = 0; i < actions.Length; ++i) {
                Debug.Log($"Action {i}: {actions[i].actionId}");
            }
        }

        private EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }
        
        private static void RunSystems(SimpleList<ComponentSystemBase> systems, int frames) {
            for (int i = 0; i < frames; ++i) {
                for (int systemIndex = 0; systemIndex < systems.Count; ++systemIndex) {
                    systems[systemIndex].Update();
                }
            }
        }
    }
}