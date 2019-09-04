using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(EndPresentationEntityCommandBufferSystem))]
    public class DestroyUnownedEntityReferencesSystem : ComponentSystem {
        private EntityQuery query;
        
        private ArchetypeChunkEntityType entityType;
        private ArchetypeChunkComponentType<EntityReference> referenceType;
    
        private EndPresentationEntityCommandBufferSystem barrier;
    
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(EntityReference));
            this.barrier = this.World.GetOrCreateSystem<EndPresentationEntityCommandBufferSystem>();
        }
    
        protected override void OnUpdate() {
            this.entityType = GetArchetypeChunkEntityType();
            this.referenceType = GetArchetypeChunkComponentType<EntityReference>(true);
    
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            // Collection of referenced entities
            NativeHashMap<Entity, byte> referencedEntities = new NativeHashMap<Entity, byte>(10, Allocator.TempJob);
    
            // Note here that we store the referenced entities here so that we can check if they
            // are still referenced by another entities before destroying them
            StoreReferencedEntities(ref chunks, ref referencedEntities);
            DestroyUnownedReferences(ref chunks, ref referencedEntities);
            
            referencedEntities.Dispose();
            chunks.Dispose();
        }
    
        private void StoreReferencedEntities(ref NativeArray<ArchetypeChunk> chunks, ref NativeHashMap<Entity, byte> referencedEntities) {
            for (int i = 0; i < chunks.Length; ++i) {
                StoreReferencedEntities(chunks[i], ref referencedEntities);
            }
        }
    
        private void StoreReferencedEntities(ArchetypeChunk chunk,
            ref NativeHashMap<Entity, byte> referencedEntities) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<EntityReference> references = chunk.GetNativeArray(this.referenceType);
            for (int i = 0; i < references.Length; ++i) {
                EntityReference entityReference = references[i];
                
                if (this.EntityManager.Exists(entityReference.owner)) {
                    referencedEntities.AddOrReplace(entities[i], (byte)0);
                }
            }
        }
    
        private void DestroyUnownedReferences(ref NativeArray<ArchetypeChunk> chunks,
            ref NativeHashMap<Entity, byte> referencedEntities) {
            for (int i = 0; i < chunks.Length; ++i) {
                DestroyUnownedReferences(chunks[i], ref referencedEntities);
            }
        }
    
        private void DestroyUnownedReferences(ArchetypeChunk chunk, ref NativeHashMap<Entity, byte> referencedEntities) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<EntityReference> references = chunk.GetNativeArray(this.referenceType);
            for (int i = 0; i < references.Length; ++i) {
                EntityReference entityReference = references[i];
                
                if (!this.EntityManager.Exists(entityReference.owner) && !referencedEntities.TryGetValue(entities[i], out _)) {
                    // Owner no longer exists and the referenced Entity is not referenced by anyone.
                    // We destroy the reference as it is no longer being pointed to
                    this.PostUpdateCommands.DestroyEntity(entities[i]);
                }
            }
        }
    }
}