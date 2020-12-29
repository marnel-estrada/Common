using System.Collections.Generic;

using Common;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(GoapPlanningSystem))]
    public class StartConditionResolverSystem : ComponentSystem {
        private EntityQuery group;

        private EntityTypeHandle entityType;
        private ComponentTypeHandle<ActionsSearch> searchType;

        private ComponentDataFromEntity<PlanRequest> allRequests;
        private ComponentDataFromEntity<GoapAgent> allAgents;
        private ComponentDataFromEntity<EcsHashMap<ushort, ByteBool>> allConditionResultMaps;
        private ComponentDataFromEntity<ActionsSearch> allSearches;
        private ComponentDataFromEntity<ConditionHashSet> allConditionHashSet;

        private GoapPlanningSystem planningSystem;

        protected override void OnCreate() {
            this.group = GetEntityQuery(typeof(ActionsSearch), typeof(ResolveNextCondition));
            this.planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
        }

        protected override void OnUpdate() {
            this.entityType = GetEntityTypeHandle();
            this.searchType = GetComponentTypeHandle<ActionsSearch>();

            this.allRequests = GetComponentDataFromEntity<PlanRequest>();
            this.allAgents = GetComponentDataFromEntity<GoapAgent>();
            this.allConditionResultMaps = GetComponentDataFromEntity<EcsHashMap<ushort, ByteBool>>();
            this.allSearches = GetComponentDataFromEntity<ActionsSearch>();
            this.allConditionHashSet = GetComponentDataFromEntity<ConditionHashSet>();

            NativeArray<ArchetypeChunk> chunks = this.group.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private NativeArray<Entity> entities;
        private NativeArray<ActionsSearch> searches;

        private void Process(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.searches = chunk.GetNativeArray(this.searchType);
            
            for (int i = 0; i < chunk.Count; ++i) {
                Process(i);
                
                // We remove this component so it will no longer be processed by this system
                this.PostUpdateCommands.RemoveComponent<ResolveNextCondition>(this.entities[i]);
            }
        }

        private void Process(int index) {
            ActionsSearch search = this.searches[index];
            PlanRequest request = this.allRequests[search.planRequestEntity];
            
            // Skip conditions that were already resolved by resolution or by parent actions
            MoveUntilUnresolvedCondition(index, ref search);

            if (search.actionSearchDone) {
                // Already done
                return;
            }
            
            // Check if current condition has a resolver
            GoapAgent agent = this.allAgents[request.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            Entity actionSearchEntity = this.entities[index];
            
            // Create the resolver entity if a preparation exists
            Option<IConditionResolverComposer> resolverPreparation = domain.GetResolver(search.CurrentTargetCondition.id);
            resolverPreparation.Match(new CreateResolverEntityMatcher(this, index, request.agentEntity, search, actionSearchEntity));
        }

        private struct CreateResolverEntityMatcher : IOptionMatcher<IConditionResolverComposer> {
            private readonly StartConditionResolverSystem system;
            private readonly int index;
            private readonly Entity agentEntity;
            private readonly ActionsSearch search;
            private readonly Entity actionSearchEntity;

            public CreateResolverEntityMatcher(StartConditionResolverSystem system, int index, Entity agentEntity, ActionsSearch search, Entity actionSearchEntity) {
                this.system = system;
                this.index = index;
                this.agentEntity = agentEntity;
                this.search = search;
                this.actionSearchEntity = actionSearchEntity;
            }

            public void OnSome(IConditionResolverComposer composer) {
                this.system.CreateResolverEntity(this.agentEntity, this.search, this.actionSearchEntity, composer);
                
                // After the preparation adds the needed components for the resolution,
                // we wait for the resolver system to resolve the value.
            }

            public void OnNone() {
                // This means that there are no condition resolvers for the current condition
                // We search for actions
                // We reset the action index as it is looking for a new action
                ActionsSearch searchCopy = this.search;
                searchCopy.currentActionIndex = -1;
                this.system.searches[this.index] = searchCopy; // Modify
                this.system.PostUpdateCommands.AddComponent(this.actionSearchEntity, new CheckSearchAction());
            }
        }

        private void MoveUntilUnresolvedCondition(int index, ref ActionsSearch search) {
            PlanRequest request = this.allRequests[search.planRequestEntity];
            
            while (search.currentConditionIndex < search.targetConditions.Count) {
                // Move to next condition to search
                search.currentConditionIndex += 1;
                
                if (search.currentConditionIndex >= search.targetConditions.Count) {
                    // It's already the last condition. All condition searches are done.
                    MarkSuccess(index, ref search, ref request);
                    return;
                }

                if (!IsCurrentTargetConditionResolved(index, ref search, ref request)) {
                    // The current target condition has not been resolved yet
                    // It has to be searched either through condition resolution or actions
                    break;
                }
            }
            
            this.searches[index] = search; // Modify the data as currentConditionIndex is moved
        }

        private bool IsCurrentTargetConditionResolved(int index, ref ActionsSearch search, ref PlanRequest request) {
            EcsHashMapWrapper<ushort, ByteBool> conditionsMap =
                new EcsHashMapWrapper<ushort, ByteBool>(request.agentEntity, this.allConditionResultMaps,
                    this.EntityManager);
            CommonEcs.Maybe<ByteBool> conditionValue = conditionsMap.Find(search.CurrentTargetCondition.id);
            Entity actionSearchEntity = this.entities[index];

            bool resolvedByCondition =
                conditionValue.HasValue && conditionValue.Value == search.CurrentTargetCondition.value;

            // The current target condition is resolved if it is resolved by condition resolvers 
            // or through parent actions
            return resolvedByCondition || HasBeenSatisfiedInAncestry(search.CurrentTargetCondition, actionSearchEntity);
        }
        
        private bool HasBeenSatisfiedInAncestry(Condition condition, Entity searchEntity) {
            Entity parentSearch = searchEntity;

            do {
                ConditionHashSet conditionHashSet = this.allConditionHashSet[parentSearch];
                if (conditionHashSet.Contains(condition)) {
                    return true;
                }

                parentSearch = this.allSearches[parentSearch].parentSearch;
            } while (parentSearch != Entity.Null);

            return false;
        }

        private void MarkSuccess(int index, ref ActionsSearch currentSearch, ref PlanRequest request) {
            currentSearch.MarkAsSuccess();
            this.searches[index] = currentSearch; // Modify search data
            
            if (currentSearch.IsRoot) {
                // It's already the root search. We mark the related plan request as success as well.
                request.status = GoapStatus.SUCCESS;
                Entity planRequestEntity = currentSearch.planRequestEntity;
                this.allRequests[planRequestEntity] = request; // Modify request data
                
                // Copy all actions in search to the plan request
                CopyActionsToPlanRequest(index, currentSearch);
                
                // We add ExecutePlan component to proceed to plan execution
                DynamicBuffer<ActionEntry> planActionList =
                    this.EntityManager.GetBuffer<ActionEntry>(planRequestEntity);
                this.PostUpdateCommands.AddComponent(planRequestEntity, new PlanExecution(planActionList.Length));
                this.PostUpdateCommands.AddComponent(planRequestEntity, new ExecuteNextAction());
            } else {
                // Add the actions of this search to its parent
                AddActionEffectsToParent(index, ref currentSearch);
                
                // The search is not root
                // It means that it's parent is resolving an action
                ActionsSearch parentSearch = this.allSearches[currentSearch.parentSearch];
                Assertion.IsTrue(parentSearch.currentActionIndex >= 0);
                
                // Add the action being resolved by parent to the parent's action list
                GoapAgent agent = this.allAgents[request.agentEntity];
                GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
                Condition currentTargetCondition = parentSearch.CurrentTargetCondition;
                Option<IReadOnlyList<GoapAction>> actions = domain.GetActions(currentTargetCondition);
                actions.Match(new AddToParentSearch(this, currentSearch, parentSearch));
                
                // We add this component to resolve the next condition of the parent search
                this.PostUpdateCommands.AddComponent(currentSearch.parentSearch, new ResolveNextCondition());   
            }
            
            // We destroy the current search to save memory as it is already done
            this.PostUpdateCommands.DestroyEntity(this.entities[index]);
        }

        private readonly struct AddToParentSearch : IOptionMatcher<IReadOnlyList<GoapAction>> {
            private readonly StartConditionResolverSystem system;
            private readonly ActionsSearch currentSearch;
            private readonly ActionsSearch parentSearch;

            public AddToParentSearch(StartConditionResolverSystem system, ActionsSearch currentSearch, ActionsSearch parentSearch) {
                this.system = system;
                this.currentSearch = currentSearch;
                this.parentSearch = parentSearch;
            }

            public void OnSome(IReadOnlyList<GoapAction> actions) {
                DynamicBuffer<ActionEntry> parentSearchActionList = this.system.EntityManager.GetBuffer<ActionEntry>(this.currentSearch.parentSearch);
                parentSearchActionList.Add(new ActionEntry(actions[this.parentSearch.currentActionIndex].id));
            }

            public void OnNone() {
            }
        }

        private void AddActionEffectsToParent(int index, ref ActionsSearch currentSearch) {
            DynamicBuffer<ActionEntry> currentSearchActions = this.EntityManager.GetBuffer<ActionEntry>(this.entities[index]);
            if (currentSearchActions.Length <= 0) {
                // There are no actions to add
                return;
            }
            
            DynamicBuffer<ActionEntry> parentSearchActions = this.EntityManager.GetBuffer<ActionEntry>(currentSearch.parentSearch);
            ConditionHashSet conditionHashSet = this.allConditionHashSet[currentSearch.parentSearch];
            PlanRequest planRequest = this.allRequests[currentSearch.planRequestEntity];
            GoapAgent agent = this.allAgents[planRequest.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            
            for (int i = 0; i < currentSearchActions.Length; ++i) {
                parentSearchActions.Add(currentSearchActions[i]);
                GoapAction action = domain.GetAction(currentSearchActions[i].actionId);
                conditionHashSet.Add(action.effect);
            }

            this.allConditionHashSet[currentSearch.parentSearch] = conditionHashSet; // Modify the data
        }

        private void CopyActionsToPlanRequest(int index, ActionsSearch currentSearch) {
            DynamicBuffer<ActionEntry> searchActionList = this.EntityManager.GetBuffer<ActionEntry>(this.entities[index]);
            DynamicBuffer<ActionEntry> planActionList =
                this.EntityManager.GetBuffer<ActionEntry>(currentSearch.planRequestEntity);

            planActionList.Clear();

            for (int i = 0; i < searchActionList.Length; ++i) {
                planActionList.Add(searchActionList[i]);
            }
        }

        private void CreateResolverEntity(Entity agentEntity, ActionsSearch search, Entity actionSearchEntity, IConditionResolverComposer resolverPreparation) {
            Entity resolverEntity = this.PostUpdateCommands.CreateEntity();
            
            ConditionResolver resolver = new ConditionResolver(agentEntity, actionSearchEntity);
            this.PostUpdateCommands.AddComponent(resolverEntity, resolver);

            resolverPreparation.Prepare(resolverEntity, this.PostUpdateCommands);
            
            // We add reference so that resolver entities would be removed if the action search entity is removed
            EntityReference.Create(actionSearchEntity, resolverEntity, this.PostUpdateCommands);
        }
    }
}