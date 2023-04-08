using System.Collections.Generic;

using Common;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(SetSpriteLayerMaterialCleanupSystem))]
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SetSpriteLayerMaterialSystem : SystemBase {
        private EntityQuery query;
        private SharedComponentQuery<SetSpriteLayerMaterial> setMaterialQuery;
        
        private SharedComponentQuery<SpriteManager> spriteManagerQuery;
        private SharedComponentQuery<MeshRendererVessel> vesselQuery;
        
        protected override void OnCreate() {
            this.query = GetEntityQuery(typeof(SetSpriteLayerMaterial));
            this.setMaterialQuery = new SharedComponentQuery<SetSpriteLayerMaterial>(this, this.EntityManager);
            
            this.spriteManagerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
            this.vesselQuery = new SharedComponentQuery<MeshRendererVessel>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.setMaterialQuery.Update();
            this.spriteManagerQuery.Update();
            this.vesselQuery.Update();

            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            chunks.Dispose();
        }

        // Process per request
        private void Process(ArchetypeChunk chunk) {
            SetSpriteLayerMaterial setSpriteLayerMaterialRequest = this.setMaterialQuery.GetSharedComponent(ref chunk);
            Assertion.IsTrue(setSpriteLayerMaterialRequest.layerEntity != Entity.Null); // Should not be null
            
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
            
            // Run through all MeshRendererVessels as well
            IReadOnlyList<MeshRendererVessel> vessels = this.vesselQuery.SharedComponents;
            for (int i = 1; i < vessels.Count; ++i) {
                MeshRendererVessel vessel = vessels[i];
                if (vessel.SpriteLayerEntity == setSpriteLayerMaterialRequest.layerEntity) {
                    // Found a vessel that is owned by the sprite layer
                    // We set the material
                    vessel.Material = setSpriteLayerMaterialRequest.newMaterial;
                }
            }
        }
    }
}