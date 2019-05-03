namespace CommonEcs {
    using System;

    using Unity.Entities;

    using UnityEngine;

    [Serializable]
    public struct SimpleRenderer : ISharedComponentData {
        public Mesh mesh;
        public Material material;
        public Color tint; // The tint color
    }
}
