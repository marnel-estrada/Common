﻿using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// This system processes only sprites with Changed component
    /// This way, we don't update all vertices in a sprite manager
    /// This is useful for layers that are static by majority but have some few sprites that animates
    /// either by UV or color 
    /// </summary>
    [UpdateInGroup(typeof(Rendering2dSystemGroup))]
    public partial class UpdateChangedVerticesSystem : UpdateVerticesSystem {
        protected override EntityQuery ResolveQuery() {
            return GetEntityQuery(ComponentType.ReadOnly<Sprite>(),
                ComponentType.ReadOnly<SpriteManager>());
        }

        protected override bool ShouldProcess(in SpriteManager manager) {
            // We skip SpriteManagers which are AlwaysUpdateMesh
            // Their vertices will be updated by another system
            bool somethingChanged = manager.VerticesChanged || manager.UvChanged || manager.ColorsChanged ||
                                    manager.RenderOrderChanged;
            
            return !manager.AlwaysUpdateMesh && somethingChanged;
        }
    }
}
