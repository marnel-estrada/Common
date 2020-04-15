using Unity.Entities;
using Unity.Mathematics;

namespace CommonEcs {
    public struct ComputeBufferSprite : IComponentData {
        // Transform here contains xy for position and zw for scale
        public float4 transform;
        
        // xy is the uv dimension and zw is the texture offset
        private float4 uv;

        private float4 color;

        // This will be combined in a float4
        public float2 size;
        public float2 pivot;

        public Entity drawInstanceEntity;
        public int masterListIndex;

        public int layerOrder; // This has more precedence
        public float renderOrder;

        public float rotation;

        public bool transformChanged;
        public bool uvChanged;
        public bool colorChanged;
        public bool renderOrderChanged;

        private const int INVALID = -1;

        public ComputeBufferSprite(Entity drawInstanceEntity, float2 size, float2 pivot) {
            this.drawInstanceEntity = drawInstanceEntity;
            this.masterListIndex = INVALID;
            
            this.uv = new float4(1.0f, 1.0f, 0.0f, 0.0f);
            this.color = new float4(1.0f, 1.0f, 1.0f, 1.0f);
            this.size = size;
            this.pivot = pivot;

            this.layerOrder = 0;
            this.renderOrder = 0;

            this.transformChanged = false;
            this.uvChanged = false;
            this.colorChanged = false;
            this.renderOrderChanged = false;
            
            this.transform = new float4();
            this.rotation = 0;
        }

        public float4 Uv {
            get {
                return this.uv;
            }
        }

        public float4 Color {
            get {
                return this.color;
            }
            
            set {
                this.color = value;
                this.colorChanged = true;
            }
        }

        public float RenderOrder {
            get {
                return this.renderOrder;
            }
            
            set {
                this.renderOrder = value;
                this.renderOrderChanged = true;
            }
        }

        public void SetUv(float2 uvSize, float2 uvOffset) {
            this.uv = new float4(uvSize, uvOffset);
            this.uvChanged = true;
        }

        public void SetTransform(float2 position, float2 scale) {
            this.transform = new float4(position, scale);
            this.transformChanged = true;
        }

        public void SetRotation(float rotation) {
            this.rotation = rotation;
            this.transformChanged = true;
        }

        public bool SomethingChanged {
            get {
                return this.transformChanged || this.uvChanged || this.colorChanged || this.renderOrderChanged;
            }
        }

        public void ResetChangedFlags() {
            this.transformChanged = false;
            this.uvChanged = false;
            this.colorChanged = false;
            this.renderOrderChanged = false;
        }
    }
}
