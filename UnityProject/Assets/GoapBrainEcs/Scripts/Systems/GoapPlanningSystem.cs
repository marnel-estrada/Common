using Common;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    public class GoapPlanningSystem : ComponentSystem {
        private EntityQuery query;

        private EntityTypeHandle entityType;
        private ComponentTypeHandle<PlanRequest> requestType;
        private BufferTypeHandle<ActionEntry> actionEntryType;

        private ComponentDataFromEntity<GoapAgent> allAgents;
        private ComponentDataFromEntity<EcsHashMap<ushort, ByteBool>> allConditionResultMaps;
        
        private readonly DomainPool domainPool = new DomainPool();

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(PlanRequest), typeof(ActionEntry), 
                ComponentType.Exclude<PlanningStarted>());
        }

        protected override void OnUpdate() {
            this.entityType = GetEntityTypeHandle();
            this.requestType = GetComponentTypeHandle<PlanRequest>(true);
            this.actionEntryType = GetBufferTypeHandle<ActionEntry>();

            this.allAgents = GetComponentDataFromEntity<GoapAgent>();
            this.allConditionResultMaps = GetComponentDataFromEntity<EcsHashMap<ushort, ByteBool>>();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<PlanRequest> requests = chunk.GetNativeArray(this.requestType);
            BufferAccessor<ActionEntry> actionLists = chunk.GetBufferAccessor(this.actionEntryType);

            for (int i = 0; i < chunk.Count; ++i) {
                PlanRequest request = requests[i];
                
                // Reset GoapAgent
                GoapAgent agent = this.allAgents[request.agentEntity];
                this.allAgents[request.agentEntity] = agent; // Modify
                
                // Clear actions
                DynamicBuffer<ActionEntry> actionList = actionLists[i];
                actionList.Clear();
                
                // Clear condition results map
                EcsHashMapWrapper<ushort, ByteBool> conditionsMap = new EcsHashMapWrapper<ushort, ByteBool>(request.agentEntity, 
                    this.allConditionResultMaps, this.EntityManager);
                conditionsMap.Clear();
                
                // Start with searching for target goals first
                Entity planEntity = entities[i];
                CreateRootSearch(agent, planEntity);

                // We added this component so that the plan request will not be processed again
                this.PostUpdateCommands.AddComponent(planEntity, new PlanningStarted());
            }
        }

        private void CreateRootSearch(GoapAgent agent, Entity planEntity) {
            ConditionList10 searchConditions = new ConditionList10();
            AddAgentGoals(ref searchConditions, ref agent.goals);
            Entity rootSearchEntity =
                ActionsSearch.Create(this.PostUpdateCommands, searchConditions, planEntity, Entity.Null);
            
            // We add reference from plan entity to root search so that search entities related to the plan request
            // would be removed if the plan request is removed
            EntityReference.Create(planEntity, rootSearchEntity, this.PostUpdateCommands);
        }

        private void AddAgentGoals(ref ConditionList10 searchConditions, ref ConditionList5 agentGoals) {
            for (int i = 0; i < agentGoals.Count; ++i) {
                searchConditions.Add(agentGoals[i]);
            }
        }

        /// <summary>
        /// Adds a domain into the system
        /// </summary>
        /// <param name="domain"></param>
        public void Add(GoapDomain domain) {
            this.domainPool.Add(domain);
        }

        /// <summary>
        /// Returns the domain with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GoapDomain GetDomain(ushort id) {
            return this.domainPool.Get(id);
        }
    }
}