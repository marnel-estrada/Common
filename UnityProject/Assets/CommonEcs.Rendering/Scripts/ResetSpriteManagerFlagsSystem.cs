using System.Collections.Generic;

using Unity.Entities;

namespace CommonEcs {
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SpriteManagerRendererSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ResetSpriteManagerFlagsSystem : SystemBase {
        private SharedComponentQuery<SpriteManager> query;

        protected override void OnCreate() {
            this.query = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.query.Update();
            IReadOnlyList<SpriteManager> managers = this.query.SharedComponents;
            for (int i = 0; i < managers.Count; ++i) {
                if (managers[i].Prepared) {
                    managers[i].ResetFlags();
                }
            }
        }
    }
}