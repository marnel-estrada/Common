using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonEcs {
    public class ComputeBufferSpriteAuthoring : MonoBehaviour {
        public float2 size;
        public float2 pivot = new(0.5f, 0.5f);
        public Color color = Color.white;
        public string? sortingLayer;

        public int[]? uvIndices;
        
        private class Baker : Baker<ComputeBufferSpriteAuthoring> {
            public override void Bake(ComputeBufferSpriteAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new ComputeBufferSprite(authoring.size, authoring.pivot, authoring.color));
                AddComponent<ComputeBufferSprite.Changed>(entity);

                int sortingLayer = string.IsNullOrWhiteSpace(authoring.sortingLayer)
                    ? 0
                    : SortingLayer.GetLayerValueFromName(authoring.sortingLayer);
                AddSharedComponent(entity, new ComputeBufferSpriteLayer(sortingLayer));
                
                // We add this since this is used to hide a sprite
                AddComponent<Active>(entity);
                SetComponentEnabled<Active>(entity, true);

                DynamicBuffer<UvIndex> uvIndexBuffer = AddBuffer<UvIndex>(entity);
                Assert.IsFalse(authoring.uvIndices == null);
                if (authoring.uvIndices == null) {
                    return;
                }
                
                for (int i = 0; i < authoring.uvIndices.Length; i++) {
                    uvIndexBuffer.Add(new UvIndex(authoring.uvIndices[i]));
                }
            }
        }
    }
}