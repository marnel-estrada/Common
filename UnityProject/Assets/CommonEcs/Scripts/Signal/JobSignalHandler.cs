using Common;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CommonEcs {
    /// <summary>
    /// It's the same as SignalHandler but for jobs.
    /// </summary>
    public class JobSignalHandler<T> where T : struct, IComponentData {
        private readonly EntityQuery query;
        private EntityTypeHandle entityType;
        private ComponentTypeHandle<T> componentType;

        private readonly ComponentSystemBase system;
        
        public delegate JobHandle JobListener(Entity entity, T component, JobHandle inputDeps);
        private readonly SimpleList<JobListener> listeners = new SimpleList<JobListener>(1);
        
        public JobSignalHandler(ComponentSystemBase system, EntityQuery query) {
            this.system = system;
            this.query = query;
        }
        
        public void AddListener(JobListener listener) {
            this.listeners.Add(listener);
        }

        public JobHandle Update(JobHandle inputDeps) {
            this.entityType = this.system.GetEntityTypeHandle();
            this.componentType = this.system.GetComponentTypeHandle<T>();

            JobHandle lastHandle = inputDeps;

            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < chunks.Length; ++i) {
                lastHandle = Process(chunks[i], lastHandle);
            }
            
            chunks.Dispose();

            return lastHandle;
        }
        
        private JobHandle Process(ArchetypeChunk chunk, JobHandle inputDeps) {
            NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
            NativeArray<T> components = chunk.GetNativeArray(this.componentType);

            JobHandle lastHandle = inputDeps;

            int count = chunk.Count;
            for (int i = 0; i < count; ++i) {
                lastHandle = Publish(entities[i], components[i], lastHandle);
                
                // Note here that we don't destroy the signal entity right away
                // DestroySignalsSystem will be the one that handles this
            }

            return lastHandle;
        }
        
        private JobHandle Publish(Entity entity, T component, JobHandle inputDeps) {
            JobHandle lastHandle = inputDeps;
            
            for (int i = 0; i < this.listeners.Count; ++i) {
                lastHandle = this.listeners[i].Invoke(entity, component, lastHandle);
            }

            return lastHandle;
        }
    }
}