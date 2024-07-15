using System.Collections.Generic;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A system that disposes ComputeBufferSpriteManagers.
    /// </summary>
    [UpdateInGroup(typeof(ComputeBufferSpriteSystemGroup))]
    public partial class DisposeComputeBufferSpriteManagerSystem : SystemBase {
        private SharedComponentQuery<ComputeBufferSpriteManager> spriteManagerQuery;

        protected override void OnCreate() {
            this.spriteManagerQuery = new SharedComponentQuery<ComputeBufferSpriteManager>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
        }

        protected override void OnDestroy() {
            this.spriteManagerQuery.Update();
            IReadOnlyList<ComputeBufferSpriteManager> spriteManagers = this.spriteManagerQuery.SharedComponents;
            
            // Note here that we start counting from 1 since the first entry is always a default one
            // In this case, SpriteManager.internal has not been allocated. So we get a NullPointerException
            // if we try to access the default entry at 0.
            for (int i = 1; i < spriteManagers.Count; i++) {
                spriteManagers[i].Dispose();
            }
        }
    }
}