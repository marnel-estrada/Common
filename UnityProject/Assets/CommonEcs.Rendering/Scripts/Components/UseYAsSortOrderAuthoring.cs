﻿using System;

using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A component that identifies whether or not to use Y as the sort order
    /// </summary>
    [Serializable]
    public struct UseYAsSortOrder : IComponentData {
        // We may want to offset the y position such that we don't have to rely on a parent
        // transform to get the correct y value.
        // This is for cases like in Academia where characters have body sprite and head sprite.
        // If we didn't have an offset, the head sprite will use its own y position and will be sorted 
        // independently from its body.
        // We need a mechanism such that the head sprite and body sprite have the same y value.
        public float offset;

        public UseYAsSortOrder(float offset) : this() {
            this.offset = offset;
        }
    }

    public class UseYAsSortOrderAuthoring : MonoBehaviour {
        public float offset;
        
        internal class Baker : Baker<UseYAsSortOrderAuthoring> {
            public override void Bake(UseYAsSortOrderAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new UseYAsSortOrder(authoring.offset));
            }
        }
    }
}