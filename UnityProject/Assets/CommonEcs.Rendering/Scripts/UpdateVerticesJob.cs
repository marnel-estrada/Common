using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    [BurstCompile]
    public struct UpdateVerticesJob : IJobChunk {
        [ReadOnly]
        public ComponentTypeHandle<Sprite> spriteType;

        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> vertices;
            
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> uv;
            
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> uv2;
            
        [NativeDisableParallelForRestriction]
        public NativeArray<Color> colors;
        
        public uint lastSystemVersion;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            if (!chunk.DidChange(this.spriteType, this.lastSystemVersion)) {
                // This means that the sprites in the chunk have not been queried with write access
                // There must be no changes at the least
                return;
            }
            
            NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
            for (int i = 0; i < sprites.Length; ++i) {
                Sprite sprite = sprites[i];

                if (sprite.verticesChanged.Value) {
                    this.vertices[sprite.index1] = sprite.transformedV1;
                    this.vertices[sprite.index2] = sprite.transformedV2;
                    this.vertices[sprite.index3] = sprite.transformedV3;
                    this.vertices[sprite.index4] = sprite.transformedV4;
                }

                if (sprite.uvChanged.Value) {
                    this.uv[sprite.index1] = sprite.uv_1;
                    this.uv[sprite.index2] = sprite.uv_2;
                    this.uv[sprite.index3] = sprite.uv_3;
                    this.uv[sprite.index4] = sprite.uv_4;

                    this.uv2[sprite.index1] = sprite.uv2_1;
                    this.uv2[sprite.index2] = sprite.uv2_2;
                    this.uv2[sprite.index3] = sprite.uv2_3;
                    this.uv2[sprite.index4] = sprite.uv2_4;
                }

                if (sprite.colorChanged.Value) {
                    this.colors[sprite.index1] = sprite.color;
                    this.colors[sprite.index2] = sprite.color;
                    this.colors[sprite.index3] = sprite.color;
                    this.colors[sprite.index4] = sprite.color;
                }
            }
        }
    }
}