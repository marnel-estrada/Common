using CommonEcs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Common {
    /// <summary>
    /// A component that adds an AABB2d to the entity. It was made this way so that other
    /// code can then just use this one instead of maintaining their own AABB.
    /// </summary>
    public struct Aabb2Component : IComponentData {
        private readonly Aabb2 localAabb; // The local axis aligned box
        
        // Location of the position of the entity in relation to the box
        // (0.5, 0.5) is the center.
        // Note that this is a normalized value.
        public readonly float2 pivot; // (0, 0) is bottom left

        // The position of the entity. This is updated per frame by UpdateAabb2System. 
        public float2 translation;

        public Aabb2Component(float width, float height, float2 pivot) : this() {
            this.localAabb = Aabb2.FromSize(width, height);
            this.pivot = pivot;
        }

        // We don't cache the world bounds so as not to increase chunk memory
        public Aabb2 WorldBounds {
            get {
                Aabb2 worldBounds = new(this.localAabb);
                float2 size = this.localAabb.Size;
                float2 normalizedOffset = new float2(0.5f, 0.5f) - pivot;
                
                worldBounds.Translate(this.translation + normalizedOffset * size);

                return worldBounds;
            }
        }
    }

    public class Aabb2ComponentAuthoring : MonoBehaviour {
        public float2 size = new(0.2f, 0.2f);
        public float2 pivot;
        
        private class Baker : Baker<Aabb2ComponentAuthoring> {
            public override void Bake(Aabb2ComponentAuthoring authoring) {
                Aabb2Component box = new(authoring.size.x, authoring.size.y, authoring.pivot);
                AddComponent(this.GetPrimaryEntity(), box);
            }
        }
    }
}