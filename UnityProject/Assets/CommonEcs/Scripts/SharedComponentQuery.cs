using Common;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    /// <summary>
    /// A utility class that aids in SharedComponent chunk iteration
    /// </summary>
    public class SharedComponentQuery<T> where T : struct, ISharedComponentData {
        private readonly ComponentSystemBase system;
        private readonly EntityManager entityManager;
    
        [ReadOnly]
        private ArchetypeChunkSharedComponentType<T> sharedComponentType;
    
        private readonly List<T> sharedComponents = new List<T>();
        private readonly List<int> indices = new List<int>();
    
        public SharedComponentQuery(ComponentSystemBase system, EntityManager entityManager) {
            this.system = system;
            this.entityManager = entityManager;
        }
    
        /// <summary>
        /// Common update routines
        /// </summary>
        public void Update() {
            this.sharedComponentType = this.system.GetArchetypeChunkSharedComponentType<T>();
            
            this.sharedComponents.Clear();
            this.indices.Clear();
            this.entityManager.GetAllUniqueSharedComponentData(this.sharedComponents, this.indices);
        }
    
        /// <summary>
        /// Returns the shared component for the specified ArchetypeChunk
        /// There should only be one
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public T GetSharedComponent(ref ArchetypeChunk chunk) {
            int sharedComponentIndex = chunk.GetSharedComponentIndex(this.sharedComponentType);
            int uniqueIndex = this.indices.IndexOf(sharedComponentIndex);
            Assertion.Assert(uniqueIndex >= 0);
            return this.SharedComponents[uniqueIndex];
        }
        
        public IReadOnlyList<T> SharedComponents {
            get {
                return this.sharedComponents;
            }
        }
    
        public IReadOnlyList<int> Indices {
            get {
                return this.indices;
            }
        }
    }
}
