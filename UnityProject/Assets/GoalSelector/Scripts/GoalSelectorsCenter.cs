using System.Collections.Generic;

using Common;

using CommonEcs;
using CommonEcs.UtilityBrain;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace GoalSelector {
    /// <summary>
    /// Handles the parsing and management of GoalSelectorData
    /// </summary>
    public class GoalSelectorsCenter : MonoBehaviour {
        [SerializeField]
        private GoalSelectorData[] dataList;

        // These are already the parsed data
        private GoalSelector[] selectors;

        private EntityArchetype brainArchetype;
        private EntityArchetype optionArchetype;

        private void Awake() {
            this.selectors = new GoalSelector[this.dataList.Length];

            // Prepare the archetypes
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            this.brainArchetype = entityManager.CreateArchetype(typeof(UtilityBrain), 
                typeof(UtilityValueWithOption), typeof(LinkedEntityGroup));
            
            // Note here that GoalCondition is the custom data that will be need to set the main goal
            // unto its associated GoapAgent
            // We added LinkedEntityGroup here for the name
            this.optionArchetype = entityManager.CreateArchetype(typeof(UtilityOption), 
                typeof(NameReference), typeof(GoalCondition), typeof(UtilityValue), 
                typeof(LinkedEntityGroup));
            
            Parse(ref entityManager);
        }

        private void Parse(ref EntityManager entityManager) {
            for (int i = 0; i < this.dataList.Length; ++i) {
                this.selectors[i] = Parse(ref entityManager, this.dataList[i]);
            }
        }

        private static GoalSelector Parse(ref EntityManager entityManager, GoalSelectorData data) {
            GoalSelector goalSelector = new GoalSelector();
            
            // Parse each goal
            for (int i = 0; i < data.Count; ++i) {
                GoalData goalData = data.GetAt(i);
                Assertion.NotEmpty(goalData.ConditionName); // Condition name must not be empty
                Goal goal = new Goal(goalData.Id, goalData.ConditionName, goalData.ConditionValue);
                ParseConsiderations(goalData, goal);
                
                goalSelector.Add(goal);
            }
            
            // Initialize assemblers
            ReadOnlySimpleList<Goal> goals = goalSelector.Goals;
            for (int i = 0; i < goals.Count; ++i) {
                InitializeAssemblers(ref entityManager, goals[i]);
            }

            return goalSelector;
        }

        private static void ParseConsiderations(GoalData data, Goal goal) {
            IReadOnlyList<ClassData> considerationsData = data.Considerations;
            for (int i = 0; i < considerationsData.Count; i++) {
                Option<ConsiderationAssembler> assemblerInstance = 
                    TypeUtils.Instantiate<ConsiderationAssembler>(considerationsData[i], null);
                assemblerInstance.Match(new AddAssemblerToGoal(goal));
            }
        }
        
        private readonly struct AddAssemblerToGoal : IOptionMatcher<ConsiderationAssembler> {
            private readonly Goal goal;

            public AddAssemblerToGoal(Goal goal) {
                this.goal = goal;
            }

            public void OnSome(ConsiderationAssembler assembler) {
                this.goal.Add(assembler);
            }

            public void OnNone() {
            }
        }

        private static void InitializeAssemblers(ref EntityManager entityManager, Goal goal) {
            ReadOnlySimpleList<ConsiderationAssembler> assemblers = goal.Assemblers;
            for (int i = 0; i < assemblers.Count; i++) {
                assemblers[i].Init(ref entityManager);
            }
        }

        private readonly SimpleList<Entity> tempLinkedEntities = new SimpleList<Entity>();

        public Entity CreateUtilityBrainEntity(EntityManager entityManager, in Entity agent, int dataIndex) {
            this.tempLinkedEntities.Clear();
            
            Entity brainEntity = CreateBrain(ref entityManager, agent, dataIndex);
            
            // Add the brainEntity first as it is the owner of the subsequent entities
            this.tempLinkedEntities.Add(brainEntity);
            
            // Create each option
            GoalSelector selector = this.selectors[dataIndex];
            ReadOnlySimpleList<Goal> goals = selector.Goals;
            for (int i = 0; i < goals.Count; i++) {
                Entity option = CreateOption(ref entityManager, goals[i], brainEntity, i, agent);
                this.tempLinkedEntities.Add(option);
            }
            
            // Commit the linked entities
            DynamicBuffer<LinkedEntityGroup> linkedEntities = entityManager.GetBuffer<LinkedEntityGroup>(brainEntity);
            for (int i = 0; i < this.tempLinkedEntities.Count; i++) {
                linkedEntities.Add(this.tempLinkedEntities[i]);
            }
            
            return brainEntity;
        }

        private Entity CreateBrain(ref EntityManager entityManager, in Entity agent, int dataIndex) {
            Entity brainEntity = entityManager.CreateEntity(this.brainArchetype);
            
            // Note here that we pass true so that UtilityBrain will execute upon creation
            entityManager.SetComponentData(brainEntity, new UtilityBrain(agent, true));
            
            // Prepare enough slots of UtilityValueWithOption
            DynamicBuffer<UtilityValueWithOption> brainValueList = entityManager.GetBuffer<UtilityValueWithOption>(brainEntity);
            GoalSelector selector = this.selectors[dataIndex];
            for (int i = 0; i < selector.GoalCount; ++i) {
                brainValueList.Add(default);
            }

            return brainEntity;
        }

        private Entity CreateOption(ref EntityManager entityManager, Goal goal, in Entity parentBrain, int brainIndex, in Entity agent) {
            Entity option = entityManager.CreateEntity(this.optionArchetype);
            entityManager.SetComponentData(option, new UtilityOption(goal.Id, parentBrain, brainIndex));
            entityManager.SetComponentData(option, new GoalCondition(goal.ConditionName, goal.ConditionValue));
            Name.SetupName(ref entityManager, option, goal.Id);

            ReadOnlySimpleList<ConsiderationAssembler> assemblers = goal.Assemblers;
            
            // Prepare enough UtilityValue slots for considerations 
            DynamicBuffer<UtilityValue> optionValueList = entityManager.GetBuffer<UtilityValue>(option);
            for (int i = 0; i < assemblers.Count; ++i) {
                optionValueList.Add(default);    
            }
            
            // Prepare consideration entity for each assembler
            // This is needed as ConsiderationAssembler.Prepare() needs this
            NativeList<Entity> optionLinkedEntities = new NativeList<Entity>(Allocator.Temp);
            for (int i = 0; i < assemblers.Count; ++i) {
                ConsiderationAssembler assembler = assemblers[i];
                assembler.Prepare(ref entityManager, agent, option, i, ref optionLinkedEntities);
            }
            
            // Add entries from optionLinkedEntities to tempLinkedEntities
            for (int i = 0; i < optionLinkedEntities.Length; i++) {
                this.tempLinkedEntities.Add(optionLinkedEntities[i]);
            }

            return option;
        }
    }
}