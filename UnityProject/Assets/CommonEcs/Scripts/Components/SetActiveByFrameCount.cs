using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// Used to activate an entity some frames later. We may use this for some cases like rendering where we
    /// want to wait one frame before we activate the entity such that we don't see the rendering jumping in
    /// a single frame.
    ///
    /// To use, call Reset() and enable the component.
    /// </summary>
    public struct SetActiveByFrameCount : IComponentData, IEnableableComponent {
        public byte frameCountdown;

        public void Reset(byte frames) {
            this.frameCountdown = frames;
            DotsAssert.IsTrue(this.frameCountdown > 0);
        }
    }
}