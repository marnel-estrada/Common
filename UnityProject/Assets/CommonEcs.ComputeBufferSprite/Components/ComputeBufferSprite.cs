using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        public Color color;

        public int managerIndex;

        // Higher order means rendered later. Value can be negative.
        public int layerOrder;
    }
}