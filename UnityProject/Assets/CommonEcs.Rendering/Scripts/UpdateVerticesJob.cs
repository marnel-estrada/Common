using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    [BurstCompile]
    public struct UpdateVerticesJob : IJobChunk {
        [ReadOnly]
        public ArchetypeChunkComponentType<Sprite> spriteType;

        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> vertices;
            
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> uv;
            
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> uv2;
            
        [NativeDisableParallelForRestriction]
        public NativeArray<Color> colors;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Sprite> sprites = chunk.GetNativeArray(this.spriteType);
            for (int i = 0; i < sprites.Length; ++i) {
                Sprite sprite = sprites[i];
                
                this.vertices[sprite.index1] = sprite.transformedV1;
                this.vertices[sprite.index2] = sprite.transformedV2;
                this.vertices[sprite.index3] = sprite.transformedV3;
                this.vertices[sprite.index4] = sprite.transformedV4;
    
                this.uv[sprite.index1] = sprite.uv_1;
                this.uv[sprite.index2] = sprite.uv_2;
                this.uv[sprite.index3] = sprite.uv_3;
                this.uv[sprite.index4] = sprite.uv_4;
    
                this.uv2[sprite.index1] = sprite.uv2_1;
                this.uv2[sprite.index2] = sprite.uv2_2;
                this.uv2[sprite.index3] = sprite.uv2_3;
                this.uv2[sprite.index4] = sprite.uv2_4;
    
                this.colors[sprite.index1] = sprite.color;
                this.colors[sprite.index2] = sprite.color;
                this.colors[sprite.index3] = sprite.color;
                this.colors[sprite.index4] = sprite.color;
            }
        }
    }
}