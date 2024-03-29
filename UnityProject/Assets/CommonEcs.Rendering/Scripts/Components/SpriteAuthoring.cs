﻿using Common;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
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

        public float renderOrder;
        
        // We differentiate this from Sprite.renderOrder so that renderOrder would now be an ordering
        // of higher precedence than renderOrderDueToPosition.
        // This is to avoid conflict when there are sprites that are positioned the same but one must
        // be rendered on top of the other. This is the case for face and head in agents or characters.
        public float renderOrderDueToPosition;
        
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
        public int layerOrder;

        // We used BitArrayX here so we can save on bytes when using booleans
        public BitArray8 flags;

        public int LayerOrder {
            get {
                return this.layerOrder;
            }
            set {
                this.RenderOrderChanged = this.layerOrder != value;
                this.layerOrder = value;
            }
        }

        public float RenderOrder {
            get {
                return this.renderOrder;
            }
            
            set {
                // Render order changed if the new render order is not the same as the previous one
                this.RenderOrderChanged = !this.renderOrder.TolerantEquals(value);                
                this.renderOrder = value;
            }
        }

        public float RenderOrderDueToPosition {
            get => this.renderOrderDueToPosition;

            set {
                // Render order changed if the new render order is not the same as the previous one
                this.RenderOrderChanged = !this.renderOrderDueToPosition.TolerantEquals(value);
                this.renderOrderDueToPosition = value;
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

            this.VerticesChanged = true;
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

            this.UvChanged = true;
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
            
            this.UvChanged = true;
        }

        /// <summary>
        /// Sets the color of the sprite
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color) {
            this.color = color;
            this.ColorChanged = true;
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

            this.VerticesChanged = true;
        }

        public bool Active {
            get => this.flags[SpriteFlags.ACTIVE];
            set => this.flags[SpriteFlags.ACTIVE] = value;
        }

        public bool VerticesChanged {
            get => this.flags[SpriteFlags.VERTICES_CHANGED];
            set => this.flags[SpriteFlags.VERTICES_CHANGED] = value;
        }

        public bool UvChanged {
            get => this.flags[SpriteFlags.UV_CHANGED];
            set => this.flags[SpriteFlags.UV_CHANGED] = value;
        }

        public bool ColorChanged {
            get => this.flags[SpriteFlags.COLOR_CHANGED];
            set => this.flags[SpriteFlags.COLOR_CHANGED] = value;
        }

        public bool RenderOrderChanged {
            get => this.flags[SpriteFlags.RENDER_ORDER_CHANGED];
            set => this.flags[SpriteFlags.RENDER_ORDER_CHANGED] = value;
        }

        public bool Hidden {
            get => this.flags[SpriteFlags.HIDDEN];

            set {
                this.flags[SpriteFlags.HIDDEN] = value;

                // We set vertices changed here so that the transform vertices would be updated
                // Note that we add an offset to the transformed vertex positions to hide the sprite
                this.VerticesChanged = true;
            }
        }
    }

    public class SpriteAuthoring : MonoBehaviour {
        public Color color = Color.white;

        public float2 pivot = new(0.5f, 0);
        
        public float renderOrder;
        public int layerOrder;

        public bool hidden;
        
        internal class Baker : Baker<SpriteAuthoring> {
            public override void Bake(SpriteAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Renderable);
                Sprite sprite = new() {
                    color = authoring.color,
                    pivot = authoring.pivot,
                    renderOrder = authoring.renderOrder,
                    layerOrder = authoring.layerOrder,
                    Hidden = authoring.hidden
                };
                AddComponent(entity, sprite);
            }
        }
    }
}
