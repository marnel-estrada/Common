using System;

using Unity.Entities;

using UnityEngine;
using UnityEngine.Serialization;

namespace CommonEcs {
    /// <summary>
    /// A common component that can be used as a filter to debug certain entities
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct DebugEntity : IComponentData {
        [FormerlySerializedAs("isDebug")]
        [SerializeField]
        public bool enabled;

        public DebugEntity(bool enabled) {
            this.enabled = enabled;
        }
    }
}
