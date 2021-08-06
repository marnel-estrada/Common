using System;

using Unity.Entities;

using UnityEngine;

namespace CommonEcs {
    /// <summary>
    /// A common component that can be used as a filter to debug certain entities
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct DebugEntity : IComponentData {
        [SerializeField]
        public bool isDebug;

        public DebugEntity(bool isDebug) {
            this.isDebug = isDebug;
        }
    }
}
