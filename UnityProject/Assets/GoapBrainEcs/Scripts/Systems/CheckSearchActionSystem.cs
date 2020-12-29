using System.Collections.Generic;

using Common;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(EndConditionResolverSystem))]
    public class CheckSearchActionSystem : ComponentSystem {
        private EntityQuery query;

        private EntityTypeHandle entityType;
        private ComponentTypeHandle<ActionsSearch> actionsSearchType;

        private GoapPlanningSystem planningSystem;
        
        private ComponentDataFromEntity<PlanRequest> allRequests;
        private ComponentDataFromEntity<GoapAgent> allAgents;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ActionsSearch), typeof(CheckSearchAction));
            this.planningSystem = this.World.GetOrCreateSystem<GoapPlanningSystem>();
        }

        protected override void OnUpdate() {
            this.entityType = GetEntityTypeHandle();
            this.actionsSearchType = GetComponentTypeHandle<ActionsSearch>();

            this.allRequests = GetComponentDataFromEntity<PlanRequest>();
            this.allAgents = GetComponentDataFromEntity<GoapAgent>();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private NativeArray<Entity> entities;
        private NativeArray<ActionsSearch> searches;

        private void Process(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.searches = chunk.GetNativeArray(this.actionsSearchType);

            for (int i = 0; i < chunk.Count; ++i) {
                Process(i);
            }
        }

        private void Process(int index) {
            ActionsSearch search = this.searches[index];
            PlanRequest request = this.allRequests[search.planRequestEntity];
            GoapAgent agent = this.allAgents[request.agentEntity];
            GoapDomain domain = this.planningSystem.GetDomain(agent.domainId);
            
            Option<IReadOnlyList<GoapAction>> actions = domain.GetActions(search.CurrentTargetCondition);
            actions.Match(new ProcessActions(this, index));
            
            // We remove this component so it won't be processed by the system again
            this.PostUpdateCommands.RemoveComponent<CheckSearchAction>(this.entities[index]);
        }

        private readonly struct ProcessActions : IOptionMatcher<IReadOnlyList<GoapAction>> {
            private readonly CheckSearchActionSystem system;
            private readonly int index;

            public ProcessActions(CheckSearchActionSystem system, int index) {
                this.system = system;
                this.index = index;
            }

            public void OnSome(IReadOnlyList<GoapAction> actions) {
                // There are actions to satisfy the current condition
                // Start search on actions
                if (actions.Count <= 0) {
                    // No actions
                    return;
                }

                ActionsSearch search = this.system.searches[this.index];
                this.system.SearchActions(this.index, ref search, actions);
            }

            public void OnNone() {
                // There are no actions to satisfy the current condition
                // The search fails
                ActionsSearch search = this.system.searches[this.index];
                this.system.DoOnFail(this.index, ref search);
            }
        }
        
        // Routine to do on fail
        private void DoOnFail(int index, ref ActionsSearch search) {
            search.MarkAsFailed();
            this.searches[index] = search; // Modify data

            if (search.IsRoot) {
                // Already the root. Continue search on fallback goals.
                SearchFallbackGoal(index, ref search);
            } else {
                // It's not the root yet. Tell parent search to proceed to next action.
                this.PostUpdateCommands.AddComponent(search.parentSearch, new CheckSearchAction());
            }
            
            // Destroy entity as it's already done
            this.PostUpdateCommands.DestroyEntity(this.entities[index]);
        }

        private void SearchFallbackGoal(int index, ref ActionsSearch search) {
            PlanRequest request = this.allRequests[search.planRequestEntity];
            GoapAgent agent = this.allAgents[request.agentEntity];
            
            if (request.fallbackIndex + 1 >= agent.fallbackGoals.Count) {
                // No more fallback goals to search
                // Mark the request as failed
                request.status = GoapStatus.FAILED;
                this.allRequests[search.planRequestEntity] = request; // Modify the data
                return;
            }

            // Create a new root search but using the next fallback goal
            request.fallbackIndex += 1;
            this.allRequests[search.planRequestEntity] = request; // Modify the data
            
            ConditionList10 searchConditions = new ConditionList10();
            searchConditions.Add(agent.fallbackGoals[request.fallbackIndex]);
            Entity fallbackSearchEntity = ActionsSearch.Create(this.PostUpdateCommands, searchConditions, search.planRequestEntity, 
                Entity.Null);
            
            EntityReference.Create(search.planRequestEntity, fallbackSearchEntity, this.PostUpdateCommands);
        }
        
        private void SearchActions(int index, ref ActionsSearch search, IReadOnlyList<GoapAction> actions) {
            search.currentActionIndex += 1;
            this.searches[index] = search; // Modify data

            if (search.currentActionIndex >= actions.Count) {
                // This means that we've exhausted all actions and we didn't find an action that will 
                // satisfy the current condition
                DoOnFail(index, ref search);
                return;
            }

            GoapAction action = actions[search.currentActionIndex];
            Entity currentSearchEntity = this.entities[index];
            
            // Create the search entity containing the preconditions of the action
            Entity childSearchEntity = ActionsSearch.Create(this.PostUpdateCommands, action.preconditions,
                search.planRequestEntity, currentSearchEntity);
            
            // We add this relationship so that child entities will be destroyed if the parent is destroyed
            EntityReference.Create(currentSearchEntity, childSearchEntity, this.PostUpdateCommands);
        }
    }
}