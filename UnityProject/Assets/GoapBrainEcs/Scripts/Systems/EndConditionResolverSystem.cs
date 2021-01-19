using Common;

using CommonEcs;

using Unity.Collections;
using Unity.Entities;

namespace GoapBrainEcs {
    [UpdateAfter(typeof(StartConditionResolverSystem))]
    public class EndConditionResolverSystem : ComponentSystem {
        private EntityQuery query;

        private EntityTypeHandle entityType;
        private ComponentTypeHandle<ConditionResolver> resolverType;

        private ComponentDataFromEntity<ActionsSearch> allSearches;
        private ComponentDataFromEntity<EcsHashMap<ushort, ByteBool>> allConditionResultMaps;

        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(ConditionResolver));
        }

        protected override void OnUpdate() {
            this.entityType = GetEntityTypeHandle();
            this.resolverType = GetComponentTypeHandle<ConditionResolver>();

            this.allSearches = GetComponentDataFromEntity<ActionsSearch>();
            this.allConditionResultMaps = GetComponentDataFromEntity<EcsHashMap<ushort, ByteBool>>();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private NativeArray<Entity> entities;
        private NativeArray<ConditionResolver> resolvers;

        private void Process(ArchetypeChunk chunk) {
            this.entities = chunk.GetNativeArray(this.entityType);
            this.resolvers = chunk.GetNativeArray(this.resolverType);

            for (int i = 0; i < chunk.Count; ++i) {
                Process(i);
            }
        }

        private void Process(int index) {
            ConditionResolver resolver = this.resolvers[index];
            if (resolver.status == ConditionResolverStatus.RUNNING) {
                // It's still running. We skip.
                return;
            }
            
            // At this point, the status is DONE
            Assertion.IsTrue(resolver.status == ConditionResolverStatus.DONE);

            ActionsSearch search = this.allSearches[resolver.actionSearchEntity];
            Condition currentTargetCondition = search.CurrentTargetCondition;
            
            // Store the result of this search on the condition pool
            // Note that each agent has a hashmap of condition results
            EcsHashMapWrapper<ushort, ByteBool> mapWrapper = new EcsHashMapWrapper<ushort, ByteBool>(resolver.agentEntity, 
                this.allConditionResultMaps, this.EntityManager);
            mapWrapper.AddOrSet(currentTargetCondition.id, resolver.result);
                
            if (currentTargetCondition.value == resolver.result) {
                // This means that the resolver result is same as the target condition value
                // We don't have to search for actions.
                // We proceed to the next target condition of the parent
                this.PostUpdateCommands.AddComponent(resolver.actionSearchEntity, new ResolveNextCondition());
            } else {
                // The resolved condition didn't meet the target
                // Let's search for actions that meets the condition
                this.PostUpdateCommands.AddComponent(resolver.actionSearchEntity, new CheckSearchAction());
            }
            
            // We destroy the condition resolver entity so as not to retain memory
            this.PostUpdateCommands.DestroyEntity(this.entities[index]);
        }
    }
}