using CommonEcs;

using NUnit.Framework;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;
using Unity.Jobs;

using UnityEngine;

namespace GoapBrainEcs {
    [TestFixture]
    [Category("EcsHashMap")]
    public class EcsHashMapTest : ECSTestsFixture {
        private struct SomeComponent : IComponentData {
            public readonly byte value;

            public SomeComponent(byte value) {
                this.value = value;
            }
        }
        
        [Test]
        public void EntitiesCreatedInEntityCommandBufferTest() {
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

            Entity entity = buffer.CreateEntity();
            buffer.AddBuffer<BufferElement<Entity>>(entity);
            DynamicBuffer<BufferElement<Entity>> entityBuffer = buffer.SetBuffer<BufferElement<Entity>>(entity);

            for (int i = 0; i < 10; ++i) {
                entityBuffer.Add(new BufferElement<Entity>(CreateValueEntity(buffer, (byte)(i + 1))));
            }
            
            buffer.Playback(this.EntityManager);
            buffer.Dispose();
            
            this.World.GetOrCreateSystem<CheckValueSystem>().Update();
            
            Assert.Pass();
        }

        private Entity CreateValueEntity(EntityCommandBuffer buffer, byte value) {
            Entity entity = buffer.CreateEntity();
            buffer.AddComponent(entity, new SomeComponent(value));
            return entity;
        }

        private class CheckValueSystem : TemplateComponentSystem {
            private BufferTypeHandle<BufferElement<Entity>> entityBufferType;

            private ComponentDataFromEntity<SomeComponent> allValueComponents;
            
            protected override EntityQuery ComposeQuery() {
                return GetEntityQuery(typeof(BufferElement<Entity>));
            }

            protected override void BeforeChunkTraversal() {
                this.entityBufferType = GetBufferTypeHandle<BufferElement<Entity>>(true);
                this.allValueComponents = GetComponentDataFromEntity<SomeComponent>();
            }

            private BufferAccessor<BufferElement<Entity>> bufferList;

            protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
                this.bufferList = chunk.GetBufferAccessor(this.entityBufferType);
            }

            protected override void Process(int index) {
                DynamicBuffer<BufferElement<Entity>> buffer = this.bufferList[index];
                for (int i = 0; i < buffer.Length; ++i) {
                    Entity valueEntity = buffer[i].value;
                    SomeComponent component = this.allValueComponents[valueEntity];
                    Assert.IsTrue(component.value > 0);
                    Debug.Log(component.value);
                }
            }
        }

        [Test]
        public void CreateHashMapByCommandBufferTest() {
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

            Entity entity = buffer.CreateEntity();
            EcsHashMap<byte, byte>.Create(entity, buffer);
            
            buffer.Playback(this.EntityManager);
            buffer.Dispose();
            
            this.World.GetOrCreateSystem<EcsHashMapUsageSystem>().Update();
        }

        private class EcsHashMapUsageSystem : TemplateComponentSystem {
            private EntityTypeHandle entityType;
            private ComponentDataFromEntity<EcsHashMap<byte, byte>> allHashMaps;
            
            protected override EntityQuery ComposeQuery() {
                return GetEntityQuery(typeof(EcsHashMap<byte, byte>), typeof(EntityBufferElement));
            }

            protected override void BeforeChunkTraversal() {
                this.entityType = GetEntityTypeHandle();
                this.allHashMaps = GetComponentDataFromEntity<EcsHashMap<byte, byte>>();
            }

            private NativeArray<Entity> entities;

            protected override void BeforeProcessChunk(ArchetypeChunk chunk) {
                this.entities = chunk.GetNativeArray(this.entityType);
            }

            protected override void Process(int index) {
                Entity entity = this.entities[index];
                EcsHashMapWrapper<byte, byte> hashMap = new EcsHashMapWrapper<byte, byte>(entity, this.allHashMaps, 
                    this.EntityManager);
                
                // Populate
                for (byte i = 1; i <= 10; ++i) {
                    hashMap.AddOrSet(i, (byte)(i * 2));
                }
                
                // Query and assert
                for (byte i = 1; i <= 10; ++i) {
                    Maybe<byte> found = hashMap.Find(i);
                    Assert.IsTrue(found.Value == i * 2);
                    Debug.Log($"{i}: {found.Value}");
                }
            }
        }

        [Test]
        public void JobComponentSystemTest() {
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

            Entity entity = buffer.CreateEntity();
            EcsHashMap<int, int>.Create(entity, buffer);
            
            buffer.Playback(this.EntityManager);
            buffer.Dispose();
            
            this.World.GetOrCreateSystem<EcsHashMapJobSystem>().Update();
        }

        private class EcsHashMapJobSystem : JobSystemBase {
            private EntityQuery entityQuery;

            private ComponentDataFromEntity<EcsHashMap<int, int>> allHashMaps;
            private BufferFromEntity<EntityBufferElement> allBuckets;
            private BufferFromEntity<EcsHashMapEntry<int, int>> allEntryLists;

            protected override void OnCreate() {
                this.entityQuery = GetEntityQuery(typeof(EcsHashMap<int, int>), typeof(EntityBufferElement));
            }

            protected override JobHandle OnUpdate(JobHandle inputDeps) {
                this.allHashMaps = GetComponentDataFromEntity<EcsHashMap<int, int>>();
                this.allBuckets = GetBufferFromEntity<EntityBufferElement>();
                this.allEntryLists = GetBufferFromEntity<EcsHashMapEntry<int, int>>();

                NativeArray<ArchetypeChunk> chunks = this.entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
                
                PopulateJob populateJob = new PopulateJob() {
                    entityType = GetEntityTypeHandle(),
                    allHashMaps = this.allHashMaps,
                    allBuckets = this.allBuckets,
                    allEntryLists = this.allEntryLists,
                    chunks = chunks
                };

                JobHandle populateHandle = IJobExtensions.Schedule(populateJob, inputDeps);

                AssertJob assertJob = new AssertJob() {
                    entityType = GetEntityTypeHandle(),
                    allHashMaps = this.allHashMaps,
                    allBuckets = this.allBuckets,
                    allEntryLists = this.allEntryLists,
                    chunks = chunks
                };

                JobHandle assertHandle = IJobExtensions.Schedule(assertJob, populateHandle);

                DeallocateNativeArrayJob<ArchetypeChunk> deallocateJob =
                    new DeallocateNativeArrayJob<ArchetypeChunk>() {
                        array = chunks
                    };

                return IJobExtensions.Schedule(deallocateJob, assertHandle);
            }

            private const int TEST_COUNT = 10000;

            // We can't use IJobChunk here as ComponentDataFromEntity nor BufferFromEntity supports
            // parallel writing
            [BurstCompile]
            private struct PopulateJob : IJob {
                [ReadOnly]
                public EntityTypeHandle entityType;
                
                public ComponentDataFromEntity<EcsHashMap<int, int>> allHashMaps;
                public BufferFromEntity<EntityBufferElement> allBuckets;
                public BufferFromEntity<EcsHashMapEntry<int, int>> allEntryLists;

                public NativeArray<ArchetypeChunk> chunks;

                public void Execute() {
                    for (int i = 0; i < this.chunks.Length; ++i) {
                        Process(this.chunks[i]);
                    }
                }

                private void Process(ArchetypeChunk chunk) {
                    NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                    for (int i = 0; i < entities.Length; ++i) {
                        Process(entities[i]);
                    }
                }

                private void Process(Entity hashMapEntity) {
                    EcsHashMapWrapper<int, int> hashMap = new EcsHashMapWrapper<int, int>(hashMapEntity, 
                        this.allHashMaps, this.allBuckets, this.allEntryLists);
                    for (int i = 1; i <= TEST_COUNT; ++i) {
                        hashMap.AddOrSet(i, i * 2);
                    }
                }
            }

            // We can't use IJobChunk here as ComponentDataFromEntity nor BufferFromEntity supports
            // parallel writing
            private struct AssertJob : IJob {
                [ReadOnly]
                public EntityTypeHandle entityType;
                
                public ComponentDataFromEntity<EcsHashMap<int, int>> allHashMaps;
                public BufferFromEntity<EntityBufferElement> allBuckets;
                public BufferFromEntity<EcsHashMapEntry<int, int>> allEntryLists;
                
                public NativeArray<ArchetypeChunk> chunks;

                public void Execute() {
                    for (int i = 0; i < this.chunks.Length; ++i) {
                        Process(this.chunks[i]);
                    }
                }

                private void Process(ArchetypeChunk chunk) {
                    NativeArray<Entity> entities = chunk.GetNativeArray(this.entityType);
                    for (int i = 0; i < entities.Length; ++i) {
                        Process(entities[i]);
                    }
                }
                
                private void Process(Entity hashMapEntity) {
                    EcsHashMapWrapper<int, int> hashMap = new EcsHashMapWrapper<int, int>(hashMapEntity, 
                        this.allHashMaps, this.allBuckets, this.allEntryLists);
                    for (int i = 1; i <= TEST_COUNT; ++i) {
                        Maybe<int> result = hashMap.Find(i);
                        Assert.IsTrue(result.HasValue);
                        Assert.IsTrue(result.Value == i * 2);
                    }
                }
            }
        }
        
        public EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }
    }
}