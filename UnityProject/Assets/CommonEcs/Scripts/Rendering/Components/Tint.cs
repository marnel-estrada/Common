namespace CommonEcs {
    using Unity.Entities;

    using UnityEngine;

    /// <summary>
    /// A component where we can store a tint color
    /// </summary>
    public struct Tint : IComponentData {
        public Color color;

        /// <summary>
        /// Constructor with specified color
        /// </summary>
        /// <param name="color"></param>
        public Tint(Color color) {
            this.color = color;
        }
    }
}
