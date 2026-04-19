using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        public Color color;
        public float2 size;
        public float2 pivot;

        // This is the ordering of the sprite within its layer
        public int layerOrder;

        // When this is true, the sprite would be part of transparent entries that need to be sorted
        public readonly bool hasTransparentContent;

        public ComputeBufferSprite(float2 size, Color color, bool hasTransparentContent = false) : this(size, new float2(0.5f, 0.5f), color, hasTransparentContent) {
        }

        public ComputeBufferSprite(float2 size, float2 pivot, Color color, bool hasTransparentContent = false) : this() {
            this.size = size;
            this.pivot = pivot;
            this.color = color;
            this.hasTransparentContent = hasTransparentContent;
        }

        public float Width => this.size.x;
        public float Height => this.size.y;

        /// <summary>
        /// An IEnableableComponent that we use to mark a sprite as changed. Systems can
        /// then filter for this.
        /// </summary>
        public readonly struct Changed : IComponentData, IEnableableComponent {
        }
    }
}