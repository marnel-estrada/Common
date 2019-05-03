using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    [UpdateAfter(typeof(CollectedCommandsSystem))]
    [UpdateAfter(typeof(SpriteManagerRendererSystem))]
    [UpdateBefore(typeof(ResetSpriteManagerFlagsSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SetMeshToVesselSystem : ComponentSystem {
        private EntityQuery query;
        private SharedComponentQuery<SpriteManager> spriteManagerQuery;
        private SharedComponentQuery<MeshRendererVessel> vesselQuery;

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(SpriteManager), typeof(MeshRendererVessel));
            
            this.spriteManagerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
            this.vesselQuery = new SharedComponentQuery<MeshRendererVessel>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            this.vesselQuery.Update();

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                Process(chunks[i]);
            }
            
            chunks.Dispose();
        }

        private void Process(ArchetypeChunk chunk) {
            SpriteManager spriteManager = this.spriteManagerQuery.GetSharedComponent(ref chunk);
            if (!spriteManager.Prepared) {
                // Not yet prepared
                return;
            }

            if (!spriteManager.MeshChanged) {
                // Mesh has not changed. No need to bother.
                return;
            }

            // Set the changed mesh to the vessel
            MeshRendererVessel vessel = this.vesselQuery.GetSharedComponent(ref chunk);
            spriteManager.UpdateMesh(); // Note that SpriteManagerRenderSystem skips calling this
            vessel.Mesh = spriteManager.Mesh;
        }
    }
}