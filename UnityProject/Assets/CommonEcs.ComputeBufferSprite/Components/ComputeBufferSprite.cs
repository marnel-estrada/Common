using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        public Color color;

        // Higher order means rendered later. Value can be negative.
        public int layerOrder;
    }
}