using System.Collections.Generic;

using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;

using UnityEngine;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("CommonEcs.SharedComponentFilter")]
    public class SharedComponentFilterTest : ECSTestsFixture {
        private struct TestSharedComponent : ISharedComponentData {
            public int value;
        }

        private struct TestComponent : IComponentData {
            public int subValue;
        }

        private class TestSystem : ComponentSystem {
            private EntityQuery query;
            private SharedComponentQuery<TestSharedComponent> sharedComponentQuery;

            private ArchetypeChunkComponentType<TestComponent> testComponentType;
            private ArchetypeChunkSharedComponentType<TestSharedComponent> sharedComponentType;

            protected override void OnCreate() {
                this.query = GetEntityQuery(typeof(TestSharedComponent), typeof(TestComponent));
                this.sharedComponentQuery = new SharedComponentQuery<TestSharedComponent>(this, this.EntityManager);
            }

            protected override void OnUpdate() {
                this.sharedComponentQuery.Update();

                this.testComponentType = GetArchetypeChunkComponentType<TestComponent>(true);
                this.sharedComponentType = GetArchetypeChunkSharedComponentType<TestSharedComponent>();

                IReadOnlyList<TestSharedComponent> sharedComponents = this.sharedComponentQuery.SharedComponents;
                for (int i = 1; i < sharedComponents.Count; ++i) {
                    this.query.SetSharedComponentFilter(sharedComponents[i]);
                    Process(this.query.CreateArchetypeChunkArray(Allocator.TempJob));
                }
            }

            private void Process(NativeArray<ArchetypeChunk> chunks) {
                for (int i = 0; i < chunks.Length; ++i) {
                    Process(chunks[i]);
                }
                
                chunks.Dispose();
            }

            private void Process(ArchetypeChunk chunk) {
                TestSharedComponent sharedComponent = chunk.GetSharedComponentData(this.sharedComponentType, this.EntityManager);
                NativeArray<TestComponent> array = chunk.GetNativeArray(this.testComponentType);
                Debug.Log($"Shared {sharedComponent.value}: {array.Length}");
            }
        }

        [Test]
        public void TestGroupFilter() {
            // Create the shared entities
            TestSharedComponent shared1 = new TestSharedComponent { value = 1 };
            TestSharedComponent shared2 = new TestSharedComponent { value = 2 };
            
            // Create the entities
            for (int i = 0; i < 10; ++i) {
                CreateEntity(shared1);
            }

            for (int i = 0; i < 20; ++i) {
                CreateEntity(shared2);
            }
            
            // Act
            TestSystem system = this.World.CreateSystem<TestSystem>();
            system.Update();
        }

        private void CreateEntity(TestSharedComponent sharedComponent) {
            Entity entity = this.EntityManager.CreateEntity();
            this.EntityManager.AddSharedComponentData(entity, sharedComponent);
            this.EntityManager.AddComponentData(entity, new TestComponent {subValue = sharedComponent.value});
        }
        
        // Just a synonym because I hate using m_Manager
        private EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }
    }
}