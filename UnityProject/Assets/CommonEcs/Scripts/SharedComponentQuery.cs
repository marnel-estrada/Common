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
        private EntityManager entityManager;
    
        [ReadOnly]
        private SharedComponentTypeHandle<T> sharedComponentType;
    
        private readonly List<T> sharedComponents = new();
        private readonly List<int> indices = new();
    
        public SharedComponentQuery(ComponentSystemBase system, EntityManager entityManager) {
            this.system = system;
            this.entityManager = entityManager;
        }
    
        /// <summary>
        /// Common update routines
        /// </summary>
        public void Update() {
            this.sharedComponentType = this.system.GetSharedComponentTypeHandle<T>();
            
            this.sharedComponents.Clear();
            this.indices.Clear();
            this.entityManager.GetAllUniqueSharedComponentsManaged(this.sharedComponents, this.indices);
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
            Assertion.IsTrue(uniqueIndex >= 0);
            return this.sharedComponents[uniqueIndex];
        }
        
        public IReadOnlyList<T> SharedComponents => this.sharedComponents;

        public IReadOnlyList<int> Indices => this.indices;
    }
}
