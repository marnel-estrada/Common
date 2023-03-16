using Unity.Collections;
using Unity.Entities;

using Common;

namespace CommonEcs {
    public class SignalHandler<T> where T : unmanaged, IComponentData {
        private EntityQuery query;
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<T> componentType;

        private readonly ComponentSystemBase system;

        public delegate void Listener(Entity entity, T component);
        
        private readonly SimpleList<Listener> listeners = new SimpleList<Listener>(1);  

        public SignalHandler(ComponentSystemBase system, EntityQuery query) {
            this.system = system;
            this.query = query;
        }

        public void AddListener(Listener listener) {
            this.listeners.Add(listener);
        }

        public void Update() {
            this.entityType = this.system.GetEntityTypeHandle();
            this.componentType = this.system.GetComponentTypeHandle<T>();

            NativeArray<ArchetypeChunk> chunks = this.query.ToArchetypeChunkArray(Allocator.TempJob);
            try {
                for (int i = 0; i < chunks.Length; ++i) {
                    Process(chunks[i]);
                }
            } finally {
                chunks.Dispose();
            }
        }

        private void Process(ArchetypeChunk chunk) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<T> components = chunk.GetNativeArray(ref this.componentType);

            int count = chunk.Count;
            for (int i = 0; i < count; ++i) {
                // Remember not to use EntityManager on the signal handler as it will alter the
                // NativeArrays that are acquired here. It will cause array is already deallocated
                // error.
                Publish(entities[i], components[i]);
                
                // Note here that we don't destroy the signal entity right away
                // DestroySignalsSystem will be the one that handles this
            }
        }

        private void Publish(Entity entity, T component) {
            for (int i = 0; i < this.listeners.Count; ++i) {
                this.listeners[i].Invoke(entity, component);
            }
        }
    }
}