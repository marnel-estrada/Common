using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;

using UnityEngine;

namespace CommonEcs {
    [TestFixture]
    [Category("CommonEcs")]
    public class EcsHashMapTest : ECSTestsFixture {
        [Test]
        public void TestAdd() {
            TestByEntityManager<AddEntityManagerTestSystem>();
        }

        [DisableAutoCreation]
        private class AddEntityManagerTestSystem : BaseEntityManagerTestSystem {
            protected override void DoTest(ref EcsHashMapWrapper<int, int> mapWrapper) {
                mapWrapper.AddOrSet(1, 2);
                Assert.True(mapWrapper.Count == 1);

                Debug.Log(mapWrapper.Find(1).Value);
                Assert.True(mapWrapper.Find(1).Value == 2);

                mapWrapper.AddOrSet(2, 4);
                Debug.Log(mapWrapper.Find(2).Value);
                Assert.True(mapWrapper.Find(2).Value == 4);
                
                // Test replace
                mapWrapper.AddOrSet(2, 8);
                Debug.Log(mapWrapper.Find(2).Value);
                Assert.True(mapWrapper.Find(2).Value == 8);
            }
        }

        [Test]
        public void TestRemove() {
            TestByEntityManager<RemoveEntityManagerTestSystem>();
        }

        [DisableAutoCreation]
        private class RemoveEntityManagerTestSystem : BaseEntityManagerTestSystem {
            protected override void DoTest(ref EcsHashMapWrapper<int, int> mapWrapper) {
                mapWrapper.AddOrSet(1, 2);
                mapWrapper.AddOrSet(3, 6);
                mapWrapper.AddOrSet(5, 10);
                Assert.True(mapWrapper.Count == 3);

                mapWrapper.Remove(3);
                Assert.True(mapWrapper.Count == 2);

                // Should have no value
                Maybe<int> result = mapWrapper.Find(3);
                Assert.False(result.HasValue);
                ;

                Assert.True(mapWrapper.Find(5).Value == 10);
                Assert.True(mapWrapper.Find(1).Value == 2);

                mapWrapper.Remove(1);
                mapWrapper.Remove(5);

                Assert.True(mapWrapper.Count == 0);
            }
        }

        [Test]
        public void TestEnumeration() {
            TestByEntityManager<EnumerationEntityManagerTestSystem>();
        }

        [DisableAutoCreation]
        private class EnumerationEntityManagerTestSystem : BaseEntityManagerTestSystem {
            protected override void DoTest(ref EcsHashMapWrapper<int, int> mapWrapper) {
                for (int i = 0; i < 10; ++i) {
                    mapWrapper.AddOrSet(i, i * 2);
                }

                int count = 0;
                foreach (EcsHashMapEntry<int, int> entry in mapWrapper) {
                    Debug.Log($"{entry.key}: {entry.value}");
                    ++count;
                }
                Assert.True(count == 10);

                // Remove some items
                mapWrapper.Remove(3);
                mapWrapper.Remove(7);
                
                Debug.Log("---"); // Spacer

                count = 0;
                foreach (EcsHashMapEntry<int, int> entry in mapWrapper) {
                    Debug.Log($"{entry.key}: {entry.value}");
                    ++count;
                }
                Assert.True(count == 8);
            }
        }

        [Test]
        public void TestClear() {
            TestByEntityManager<ClearEntityManagerTestSystem>();
        }

        [DisableAutoCreation]
        private class ClearEntityManagerTestSystem : BaseEntityManagerTestSystem {
            protected override void DoTest(ref EcsHashMapWrapper<int, int> mapWrapper) {
                for (int i = 0; i < 10; ++i) {
                    mapWrapper.AddOrSet(i, i * 2);
                }

                int count = 0;
                foreach (EcsHashMapEntry<int, int> entry in mapWrapper) {
                    Debug.Log($"{entry.key}: {entry.value}");
                    ++count;
                }
                Assert.True(count == 10);
                
                mapWrapper.Clear();
                Assert.True(mapWrapper.Count == 0);
                
                Debug.Log("---"); // Spacer
                
                // Try to iterate again. It should not.
                count = 0;
                foreach (EcsHashMapEntry<int, int> entry in mapWrapper) {
                    Debug.Log($"{entry.key}: {entry.value}");
                    ++count;
                }
                Assert.True(count == 0);
            }
        }

        // We don't add this test for now because we have no way of resolving bucket entities
        // when they are created using EntityCommandBuffer
        //[Test]
        public void CreateByCommandBuffer() {
            // We need a barrier for the EntityCommandBuffer to flush
            CreateByCommandBufferSystem barrier =
                this.World.GetOrCreateSystem<CreateByCommandBufferSystem>();
            EntityCommandBuffer commandBuffer = barrier.CreateCommandBuffer();
            Entity entity = this.EntityManager.CreateEntity();
            EcsHashMap<int, int>.Create(entity, commandBuffer);
            barrier.Update(); // this will flush the buffer

            // Test created hashmap
            TestHashMapByCommandBufferSystem testSystem =
                this.World.GetOrCreateSystem<TestHashMapByCommandBufferSystem>();
            testSystem.Update();
        }

        [DisableAutoCreation]
        private class CreateByCommandBufferSystem : EntityCommandBufferSystem {
        }

        [DisableAutoCreation]
        private class TestHashMapByCommandBufferSystem : ComponentSystem {
            private EntityQuery query;
            private ArchetypeChunkComponentType<EcsHashMap<int, int>> mapType;
            private ArchetypeChunkBufferType<EntityBufferElement> bucketsType;

            protected override void OnCreate() {
                this.query = GetEntityQuery(typeof(EcsHashMap<int, int>), typeof(EntityBufferElement));
            }

            protected override void OnUpdate() {
                this.mapType = GetArchetypeChunkComponentType<EcsHashMap<int, int>>();
                this.bucketsType = GetArchetypeChunkBufferType<EntityBufferElement>();
                
                NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
                for (int i = 0; i < chunks.Length; ++i) {
                    Process(chunks[i]);
                }
                
                chunks.Dispose();
            }

            private BufferAccessor<EntityBufferElement> bucketLists;
            
            private void Process(ArchetypeChunk chunk) {
                NativeArray<EcsHashMap<int, int>> maps = chunk.GetNativeArray(this.mapType);
                this.bucketLists = chunk.GetBufferAccessor(this.bucketsType);
                Debug.Log($"Maps: {maps.Length}, {this.bucketLists[0].Length}");

                for (int i = 0; i < chunk.Count; ++i) {
                    Process(i);
                }
            }

            private void Process(int index) {
                DynamicBuffer<EntityBufferElement> buckets = this.bucketLists[index];
                for (int i = 0; i < buckets.Length; ++i) {
                    Debug.Log($"{i}: {buckets[i].entity}");
                    DynamicBuffer<BufferElement<EcsHashMapEntry<int, int>>> entryList = this.EntityManager.GetBuffer<BufferElement<EcsHashMapEntry<int, int>>>(buckets[i].entity);
                    Debug.Log($"entryList: {entryList.Length}");
                }
            }
        }

        private void TestByEntityManager<T>() where T : BaseEntityManagerTestSystem {
            Entity entity = this.EntityManager.CreateEntity();
            EcsHashMap<int, int>.Create(entity, this.EntityManager);

            T system = this.World.GetOrCreateSystem<T>();
            system.Update();
        }

        private abstract class BaseEntityManagerTestSystem : ComponentSystem {
            private EntityQuery query;

            private ComponentDataFromEntity<EcsHashMap<int, int>> allHashMaps;

            protected override void OnCreate() {
                this.query = GetEntityQuery(typeof(EcsHashMap<int, int>));
            }

            protected override void OnUpdate() {
                this.allHashMaps = GetComponentDataFromEntity<EcsHashMap<int, int>>();

                this.Entities.With(this.query).ForEach(delegate(Entity entity, ref EcsHashMap<int, int> map) {
                    EcsHashMapWrapper<int, int> mapWrapper = new EcsHashMapWrapper<int, int>(entity,
                        this.allHashMaps, this.EntityManager);
                    DoTest(ref mapWrapper);
                });
            }

            protected abstract void DoTest(ref EcsHashMapWrapper<int, int> mapWrapper);
        }

        // Just a synonym since I don't like using m_Manager
        private EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }
    }
}