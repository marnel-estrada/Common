using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DestroyUnownedEntityReferencesSystem : ComponentSystem {
        private EntityQuery query;
        
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<EntityReference> referenceType;
    
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(EntityReference));
        }
    
        protected override void OnUpdate() {
            this.entityType = GetEntityTypeHandle();
            this.referenceType = GetComponentTypeHandle<EntityReference>(true);
    
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            // Collection of referenced entities
            NativeParallelHashMap<Entity, byte> referencedEntities = new NativeParallelHashMap<Entity, byte>(10, Allocator.TempJob);
    
            // Note here that we store the referenced entities here so that we can check if they
            // are still referenced by another entities before destroying them
            StoreReferencedEntities(ref chunks, ref referencedEntities);
            DestroyUnownedReferences(ref chunks, ref referencedEntities);
            
            referencedEntities.Dispose();
            chunks.Dispose();
        }
    
        private void StoreReferencedEntities(ref NativeArray<ArchetypeChunk> chunks, ref NativeParallelHashMap<Entity, byte> referencedEntities) {
            for (int i = 0; i < chunks.Length; ++i) {
                StoreReferencedEntities(chunks[i], ref referencedEntities);
            }
        }
    
        private void StoreReferencedEntities(ArchetypeChunk chunk,
            ref NativeParallelHashMap<Entity, byte> referencedEntities) {
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
            ref NativeParallelHashMap<Entity, byte> referencedEntities) {
            for (int i = 0; i < chunks.Length; ++i) {
                DestroyUnownedReferences(chunks[i], ref referencedEntities);
            }
        }
    
        private void DestroyUnownedReferences(ArchetypeChunk chunk, ref NativeParallelHashMap<Entity, byte> referencedEntities) {
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