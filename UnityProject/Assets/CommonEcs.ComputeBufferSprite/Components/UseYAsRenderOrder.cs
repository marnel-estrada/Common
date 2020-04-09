using System;

using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A component that identifies whether or not to use Y as the render order of the sprite
    /// </summary>
    [Serializable]
    public struct UseYAsRenderOrder : IComponentData {
        // We may want to offset the y position such that we don't have to rely on a parent
        // transform to get the correct y value.
        // This is for cases like in Academia where characters have body sprite and head sprite.
        // If we didn't have an offset, the head sprite will use its own y position and will be sorted 
        // independently from its body.
        // We need a mechanism such that the head sprite and body sprite have the same y value.
        public float offset;

        public UseYAsRenderOrder(float offset) {
            this.offset = offset;
        }
    }
}