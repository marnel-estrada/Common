using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(SetSpriteLayerMaterialCleanupSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SetSpriteLayerMaterialSystem : ComponentSystem {
        private EntityQuery query;
        private SharedComponentQuery<SetSpriteLayerMaterial> setMaterialQuery;
        
        private SharedComponentQuery<SpriteManager> spriteManagerQuery;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(SetSpriteLayerMaterial));
            this.setMaterialQuery = new SharedComponentQuery<SetSpriteLayerMaterial>(this, this.EntityManager);
            this.spriteManagerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.setMaterialQuery.Update();
            this.spriteManagerQuery.Update();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            chunks.Dispose();
        }

        // Process per request
        private void Process(ArchetypeChunk chunk) {
            SetSpriteLayerMaterial setSpriteLayerMaterialRequest = this.setMaterialQuery.GetSharedComponent(ref chunk);
            
            // Run through all SpriteManagers and set the material to those belonging to the specified layer
            // Note here that we start iteration from 1 because the first SpriteManager is the default value
            IReadOnlyList<SpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            for (int i = 1; i < spriteManagers.Count; ++i) {
                SpriteManager spriteManager = spriteManagers[i];
                if (spriteManager.SpriteLayerEntity == setSpriteLayerMaterialRequest.layerEntity) {
                    // The SpriteManager is one of the SpriteManagers under such layer
                    // We change the material
                    spriteManager.SetMaterial(setSpriteLayerMaterialRequest.newMaterial);
                }
            }
        }
    }
}