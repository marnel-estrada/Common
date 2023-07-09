using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Note that we removed this from Rendering2dSystemGroup since it is using SignalHandlerComponentSystem.
    /// The system that handles the destruction of signals is in simulation group. 
    /// </summary>
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    public partial class SetSpriteLayerVesselsEnabledSystem : SignalHandlerComponentSystem<SetSpriteLayerVesselsEnabled> {
        private SharedComponentQuery<MeshRendererVessel> vesselQuery;
        
        protected override void OnCreate() {
            base.OnCreate();
            this.vesselQuery = new SharedComponentQuery<MeshRendererVessel>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.vesselQuery.Update();
            base.OnUpdate();
        }

        protected override void OnDispatch(Entity entity, SetSpriteLayerVesselsEnabled signalComponent) {
            // Traverse through all vessels
            IReadOnlyList<MeshRendererVessel> vessels = this.vesselQuery.SharedComponents;
            for (int i = 1; i < vessels.Count; ++i) {
                MeshRendererVessel vessel = vessels[i];
                if (vessel.SpriteLayerEntity == signalComponent.spriteLayerEntity) {
                    // Found a vessel that is owned by the sprite layer
                    // We set enabled
                    vessel.Enabled = signalComponent.enabled;
                }
            }
        }
    }
}