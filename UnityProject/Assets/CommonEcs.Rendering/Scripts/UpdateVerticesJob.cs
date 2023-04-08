using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
        
        private static readonly float3 OFF_SCREEN = new(1000, 1000, 1000);

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
            if (!chunk.DidChange(ref this.spriteType, this.lastSystemVersion)) {
                // This means that the sprites in the chunk have not been queried with write access
                // There must be no changes at the least
                return;
            }
            
            NativeArray<Sprite> sprites = chunk.GetNativeArray(ref this.spriteType);
            ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (enumerator.NextEntityIndex(out int i)) {
                Sprite sprite = sprites[i];

                if (sprite.VerticesChanged) {
                    if (sprite.Hidden) {
                        // We add off screen position if the sprite was set to not visible
                        this.vertices[sprite.index1] = sprite.transformedV1 + OFF_SCREEN;
                        this.vertices[sprite.index2] = sprite.transformedV2 + OFF_SCREEN;
                        this.vertices[sprite.index3] = sprite.transformedV3 + OFF_SCREEN;
                        this.vertices[sprite.index4] = sprite.transformedV4 + OFF_SCREEN;
                    } else {
                        // Sprite is visible. No need to add off screen offset.
                        this.vertices[sprite.index1] = sprite.transformedV1;
                        this.vertices[sprite.index2] = sprite.transformedV2;
                        this.vertices[sprite.index3] = sprite.transformedV3;
                        this.vertices[sprite.index4] = sprite.transformedV4;
                    }
                }

                if (sprite.UvChanged) {
                    this.uv[sprite.index1] = sprite.uv_1;
                    this.uv[sprite.index2] = sprite.uv_2;
                    this.uv[sprite.index3] = sprite.uv_3;
                    this.uv[sprite.index4] = sprite.uv_4;

                    this.uv2[sprite.index1] = sprite.uv2_1;
                    this.uv2[sprite.index2] = sprite.uv2_2;
                    this.uv2[sprite.index3] = sprite.uv2_3;
                    this.uv2[sprite.index4] = sprite.uv2_4;
                }

                if (sprite.ColorChanged) {
                    this.colors[sprite.index1] = sprite.color;
                    this.colors[sprite.index2] = sprite.color;
                    this.colors[sprite.index3] = sprite.color;
                    this.colors[sprite.index4] = sprite.color;
                }
            }
        }
    }
}