using System;

using Unity.Collections;
using Unity.Entities;

namespace CommonEcs {
    public struct EcsHashMap<K, V> : IComponentData 
        where K : unmanaged, IEquatable<K> 
        where V : unmanaged {
        public int count;
        
        public const byte BUCKET_COUNT = 16;

        /// <summary>
        /// Prepares the EcsHashMap components on the specified entity
        /// </summary>
        /// <param name="commandBuffer"></param>
        public static void Create(Entity entity, EntityCommandBuffer commandBuffer) {
            commandBuffer.AddComponent(entity, new EcsHashMap<K, V>());
            commandBuffer.AddBuffer<EntityBufferElement>(entity);
            DynamicBuffer<EntityBufferElement> buckets = commandBuffer.SetBuffer<EntityBufferElement>(entity);
            
            // Prepare the buckets
            for (int i = 0; i < BUCKET_COUNT; ++i) {
                Entity listEntity = CreateValueList(commandBuffer);
                buckets.Add(new EntityBufferElement(listEntity));
            }
        }

        private static Entity CreateValueList(EntityCommandBuffer commandBuffer) {
            Entity listEntity = commandBuffer.CreateEntity();
            commandBuffer.AddBuffer<EcsHashMapEntry<K, V>>(listEntity);
            return listEntity;
        }
        
        /// <summary>
        /// Prepares the EcsHashMap components on the specified entity
        /// </summary>
        /// <param name="entityManager"></param>
        public static void Create(Entity entity, EntityManager entityManager) {
            entityManager.AddComponentData(entity, new EcsHashMap<K, V>());
            entityManager.AddBuffer<EntityBufferElement>(entity);
            
            NativeArray<Entity> tempList = new NativeArray<Entity>(BUCKET_COUNT, Allocator.TempJob);
            
            // Prepare the buckets
            // We add in a temporary array first because EntityManager is disrupted on the call to CreateValueList()
            // We can't add it to the DynamicBuffer right away
            for (int i = 0; i < BUCKET_COUNT; ++i) {
                tempList[i] = CreateValueList(entityManager);
            }
            
            DynamicBuffer<EntityBufferElement> buckets = entityManager.GetBuffer<EntityBufferElement>(entity);
            for (int i = 0; i < BUCKET_COUNT; ++i) {
                buckets.Add(new EntityBufferElement(tempList[i]));
            }

            tempList.Dispose();
        }

        private static Entity CreateValueList(EntityManager entityManager) {
            Entity listEntity = entityManager.CreateEntity();
            entityManager.AddBuffer<EcsHashMapEntry<K, V>>(listEntity);
            return listEntity;
        }
    }
}