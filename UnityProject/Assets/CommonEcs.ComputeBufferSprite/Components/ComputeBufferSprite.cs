using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        // xy is the uv dimension and zw is the texture offset
        public float4 uv;

        public float4 color;
        
        // This will be combined in a float4
        public float2 size;
        public float2 anchor;

        public float renderOrder;

        public bool uvChanged;
        public bool colorChanged;
        public bool renderOrderChanged;

        public ComputeBufferSprite(float2 size, float2 anchor) {
            this.uv = new float4(1.0f, 1.0f, 0.0f, 0.0f);
            this.color = new float4(1.0f, 1.0f, 1.0f, 1.0f);
            this.size = size;
            this.anchor = anchor;

            this.renderOrder = 0;

            this.uvChanged = false;
            this.colorChanged = false;
            this.renderOrderChanged = false;
        }

        public void SetUv(float2 uvSize, float2 uvOffset) {
            this.uv = new float4(uvSize, uvOffset);
        }
    }
}
