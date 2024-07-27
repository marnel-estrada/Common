using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CommonEcs {
    public class ComputeBufferSpriteAuthoring : MonoBehaviour {
        public float2 size;
        public Color color = Color.white;
        public int layerOrder;

        public int[]? uvIndices;
        
        private class Baker : Baker<ComputeBufferSpriteAuthoring> {
            public override void Bake(ComputeBufferSpriteAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new ComputeBufferSprite(authoring.size, authoring.color) {
                    layerOrder = authoring.layerOrder
                });

                DynamicBuffer<UvIndex> uvIndexBuffer = AddBuffer<UvIndex>(entity);
                Assert.IsFalse(authoring.uvIndices == null);
                if (authoring.uvIndices == null) {
                    return;
                }
                
                Assert.IsTrue(authoring.uvIndices.Length > 0);
                for (int i = 0; i < authoring.uvIndices.Length; i++) {
                    uvIndexBuffer.Add(new UvIndex(authoring.uvIndices[i]));
                }
            }
        }
    }
}