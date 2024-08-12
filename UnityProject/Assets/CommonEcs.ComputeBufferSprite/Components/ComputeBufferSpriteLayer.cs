using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Used to group ComputeBufferSprites by layer. This can be used to query sprites at
    /// a certain layer like for example, if we want to hide/show sprites at a certain layer.
    /// </summary>
    public struct ComputeBufferSpriteLayer : ISharedComponentData {
        public readonly int value;

        public ComputeBufferSpriteLayer(int value) {
            this.value = value;
        }
    }
}