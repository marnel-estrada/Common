using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    [UpdateBefore(typeof(SpriteManagerRendererSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SetSpriteLayerVesselsEnabledSystem : SignalHandlerComponentSystem<SetSpriteLayerVesselsEnabled> {
        private SharedComponentQuery<MeshRendererVessel> vesselQuery;
        
        protected override void OnCreate() {
            base.OnCreate();
            this.vesselQuery = new SharedComponentQuery<MeshRendererVessel>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.vesselQuery.Update();
            base.OnUpdate();
        }

        protected override void OnDispatch(Entity entity, SetSpriteLayerVesselsEnabled signalParameter) {
            // Traverse through all vessels
            IReadOnlyList<MeshRendererVessel> vessels = this.vesselQuery.SharedComponents;
            for (int i = 1; i < vessels.Count; ++i) {
                MeshRendererVessel vessel = vessels[i];
                if (vessel.SpriteLayerEntity == signalParameter.spriteLayerEntity) {
                    // Found a vessel that is owned by the sprite layer
                    // We set enabled
                    vessel.Enabled = signalParameter.enabled;
                }
            }
        }
    }
}