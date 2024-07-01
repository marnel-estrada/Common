using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DrawComputeBufferSpritesSystem : SystemBase {
        private EntityQuery query;

        protected override void OnCreate() {
            this.query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ComputeBufferSpriteManager>()
                .Build(this);
        }
        
        private static readonly Bounds BOUNDS = new(Vector2.zero, Vector3.one * 10);

        protected override void OnUpdate() {
            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            SharedComponentTypeHandle<ComputeBufferSpriteManager> spriteManagerType = 
                GetSharedComponentTypeHandle<ComputeBufferSpriteManager>();
            
            for (int i = 0; i < chunks.Length; i++) {
                chunks[i].GetSharedComponentManaged(spriteManagerType, this.EntityManager).Draw(BOUNDS);
            }

            chunks.Dispose();
        }
    }
}