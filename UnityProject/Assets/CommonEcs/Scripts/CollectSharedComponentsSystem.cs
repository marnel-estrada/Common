using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {    
    public abstract class CollectSharedComponentsSystem<T> : SystemBase where T : struct, ISharedComponentData {
        public struct Collected : ISystemStateComponentData {
        } 

        private EntityQuery query;
        private ArchetypeChunkEntityType entityType;
        private SharedComponentQuery<T> sharedQuery;
        
        private readonly Dictionary<Entity, T> map = new Dictionary<Entity, T>(1);

        private EndSimulationEntityCommandBufferSystem barrier;

        protected override void OnCreate() {
            this.query = ResolveQuery();
            this.sharedQuery = new SharedComponentQuery<T>(this, this.EntityManager);
            this.barrier = this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        /// <summary>
        /// We made this virtual because some subclass might require a different subtractive group
        /// </summary>
        /// <returns></returns>
        protected virtual EntityQuery ResolveQuery() {
            return GetEntityQuery(this.ConstructQuery(null, new ComponentType[] {
                typeof(Collected)
            }, new ComponentType[] {
                typeof(T)
            }));
        }

        protected override void OnUpdate() {
            this.entityType = GetArchetypeChunkEntityType();
            this.sharedQuery.Update();
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            EntityCommandBuffer commandBuffer = this.barrier.CreateCommandBuffer();
            
            for(int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i], commandBuffer);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk, EntityCommandBuffer commandBuffer) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            T sharedComponent = this.sharedQuery.GetSharedComponent(ref chunk);

            for (int i = 0; i < chunk.Count; ++i) {
                Entity entity = entities[i];
                this.map[entity] = sharedComponent;
                
                // Add this component so it will no longer be processed by this system
                commandBuffer.AddComponent(entity, new Collected());
            }
        }
        
        /// <summary>
        /// Returns the component attached to the specified entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Maybe<T> Get(Entity entity) {
            if (this.map.TryGetValue(entity, out T value)) {
                return new Maybe<T>(value);
            }
            
            return Maybe<T>.Nothing;
        }

        public int Count {
            get {
                return this.map.Count;
            }
        }        
    }
}
