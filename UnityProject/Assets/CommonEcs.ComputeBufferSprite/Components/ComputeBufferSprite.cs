using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        public Color color;
        public float2 size;
        public float2 pivot;

        public ComputeBufferSprite(float2 size, Color color) : this(size, new float2(0.5f, 0.5f), color) {
        }

        public ComputeBufferSprite(float2 size, float2 pivot, Color color) : this() {
            this.size = size;
            this.pivot = pivot;
            this.color = color;
        }

        /// <summary>
        /// An IEnableableComponent that we use to mark a sprite as changed. Systems can
        /// then filter for this.
        /// </summary>
        public readonly struct Changed : IComponentData, IEnableableComponent {
        }
    }
}