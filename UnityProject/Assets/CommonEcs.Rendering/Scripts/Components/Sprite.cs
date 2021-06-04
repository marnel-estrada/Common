using System;

using Common;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    [Serializable]
    [GenerateAuthoringComponent]
    public struct Sprite : IComponentData {
        // Note here that we rearrange the variables from high number of bytes to low
        // This is so that memory is properly compacted

        // We only maintain one color for all vertices (save memory)
        public Color color;
        
        // The vertices in local space
        public float3 v1;
        public float3 v2;
        public float3 v3;
        public float3 v4;
        
        // Transformed vertices
        // Used as temporary variable to hold the result when computed in a system
        // We can't easily assign directly to a mesh while in system
        public float3 transformedV1;
        public float3 transformedV2;
        public float3 transformedV3;
        public float3 transformedV4;

        public Vector2 uv_1;
        public Vector2 uv_2;
        public Vector2 uv_3;
        public Vector2 uv_4;

        public Vector2 uv2_1;
        public Vector2 uv2_2;
        public Vector2 uv2_3;
        public Vector2 uv2_4;

        // We store the pivot here so that when we change sprites, we still have the original
        // pivot value
        public float2 pivot;
        
        public Entity spriteManagerEntity;
        
        public float width;
        public float height;

        private float renderOrder;
        
        // Indices of the vertex to the central mesh
        public int index1;
        public int index2;
        public int index3;
        public int index4;

        // The index of the sprite in SpriteManager
        public int managerIndex;

        // These two are used for sorting
        // Layer order has higher precedence than renderOrder
        // renderOrder is used for things like sorting by z position
        private int layerOrder;

        public ByteBool active;

        public ByteBool verticesChanged;
        public ByteBool uvChanged;
        public ByteBool colorChanged;
        public ByteBool renderOrderChanged;

        public int LayerOrder {
            get {
                return this.layerOrder;
            }
            set {
                this.renderOrderChanged.Value = this.layerOrder != value;
                this.layerOrder = value;
            }
        }

        public float RenderOrder {
            get {
                return this.renderOrder;
            }
            
            set {
                // Render order changed if the new render order is not the same as the previous one
                this.renderOrderChanged.Value = !this.renderOrder.TolerantEquals(value);                
                this.renderOrder = value;
            }
        }

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Init(in Entity spriteManagerEntity, float width, float height, float2 pivot) {
            this.spriteManagerEntity = spriteManagerEntity;
            
            this.width = width;
            this.height = height; 

            float halfWidth = this.width * 0.5f;
            float halfHeight = this.height * 0.5f;

            float2 offset = new float2(0.5f, 0.5f) - pivot;    
            
            this.v1 = new float3(-halfWidth + (offset.x * width), -halfHeight + (offset.y * height), 0); // Lower left
            this.v2 = new float3(halfWidth + (offset.x * width), -halfHeight + (offset.y * height), 0); // Lower right
            this.v3 = new float3(-halfWidth + (offset.x * width), halfHeight + (offset.y * height), 0); // Upper left
            this.v4 = new float3(halfWidth + (offset.x * width), halfHeight + (offset.y * height), 0); // Upper right

            this.color = ColorUtils.WHITE;
            
            // Indeces will be properly set by AddSpriteToManagerSystem
            this.index1 = 0;
            this.index2 = 0;
            this.index3 = 0;
            this.index4 = 0;
        }
        
        public void SetLocalVertices(float width, float height, float2 pivot) {
            this.width = width;
            this.height = height;
            this.pivot = pivot;
            
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            float2 offset = new float2(0.5f, 0.5f) - pivot;    
            
            this.v1 = new float3(-halfWidth + (offset.x * width), -halfHeight + (offset.y * height), 0); // Lower left
            this.v2 = new float3(halfWidth + (offset.x * width), -halfHeight + (offset.y * height), 0); // Lower right
            this.v3 = new float3(-halfWidth + (offset.x * width), halfHeight + (offset.y * height), 0); // Upper left
            this.v4 = new float3(halfWidth + (offset.x * width), halfHeight + (offset.y * height), 0); // Upper right

            this.verticesChanged.Value = true;
        }

        /// <summary>
        /// Sets the UV (the first one)
        /// </summary>
        /// <param name="lowerLeftUv"></param>
        /// <param name="uvDimension"></param>
        public void SetUv(float2 lowerLeftUv, float2 uvDimension) {
            this.uv_1 = lowerLeftUv; // Lower left
            this.uv_2 = lowerLeftUv + new float2(uvDimension.x, 0); // Lower right
            this.uv_3 = lowerLeftUv + new float2(0, uvDimension.y); // Upper left
            this.uv_4 = lowerLeftUv + uvDimension; // Upper right

            this.uvChanged.Value = true;
        }

        /// <summary>
        /// Sets the UV2
        /// </summary>
        /// <param name="lowerLeftUv2"></param>
        /// <param name="uvDimension2"></param>
        public void SetUv2(float2 lowerLeftUv2, float2 uvDimension2) {
            this.uv2_1 = lowerLeftUv2; // Lower left
            this.uv2_2 = lowerLeftUv2 + new float2(uvDimension2.x, 0); // Lower right
            this.uv2_3 = lowerLeftUv2 + new float2(0, uvDimension2.y); // Upper left
            this.uv2_4 = lowerLeftUv2 + uvDimension2; // Upper right
            
            this.uvChanged.Value = true;
        }

        /// <summary>
        /// Sets the color of the sprite
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color) {
            this.color = color;
            this.colorChanged.Value = true;
        }

        /// <summary>
        /// Transforms the sprite using the specified matrix
        /// </summary>
        /// <param name="matrix"></param>
        public void Transform(ref float4x4 matrix) {
            this.transformedV1 = math.mul(matrix, new float4(this.v1, 1)).xyz;
            this.transformedV2 = math.mul(matrix, new float4(this.v2, 1)).xyz;
            this.transformedV3 = math.mul(matrix, new float4(this.v3, 1)).xyz;
            this.transformedV4 = math.mul(matrix, new float4(this.v4, 1)).xyz;

            this.verticesChanged.Value = true;
        }
    }
}
