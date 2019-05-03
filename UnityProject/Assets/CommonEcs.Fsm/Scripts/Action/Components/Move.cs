using System;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace CommonEcs {
    [Serializable]
    public struct Move : IComponentData {
        public float3 positionFrom;
        public float3 positionTo;
        public Space space;
    }
}