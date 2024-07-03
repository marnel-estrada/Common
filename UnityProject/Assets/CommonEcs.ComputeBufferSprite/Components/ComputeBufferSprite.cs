using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        public Color color;

        public ValueTypeOption<int> managerIndex;

        // Higher order means rendered later. Value can be negative.
        public int layerOrder;

        public ComputeBufferSprite(Color color) : this() {
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