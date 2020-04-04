using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("CommonEcs.SharedComponentQuery")]
    public class SharedComponentQueryTest : ECSTestsFixture {
        private struct TestSharedComponent : ISharedComponentData {
            public int value;
        }

        // A system that uses the TestSharedComponent
        private class TestSystem : ComponentSystem {
            private EntityQuery query;

            private SharedComponentQuery<TestSharedComponent> sharedComponentQuery;

            private TestSharedComponent componentFromQuery;

            public TestSharedComponent ComponentFromQuery {
                get {
                    return this.componentFromQuery;
                }
            }

            protected override void OnCreate() {
                this.query = GetEntityQuery(typeof(TestSharedComponent));
                this.sharedComponentQuery = new SharedComponentQuery<TestSharedComponent>(this, this.EntityManager);
            }

            protected override void OnUpdate() {
                this.sharedComponentQuery.Update();
                NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
                for (int i = 0; i < chunks.Length; ++i) {
                    ArchetypeChunk chunk = chunks[i];
                    Process(ref chunk);
                }
            }

            private void Process(ref ArchetypeChunk chunk) {
                this.componentFromQuery = this.sharedComponentQuery.GetSharedComponent(ref chunk);
            }
        }

        // Just a synonym because I hate using m_Manager
        private EntityManager EntityManager {
            get {
                return this.m_Manager;
            }
        }

        [Test]
        public void GetSharedComponent_Should_Return_A_Valid_One() {
            // Arrange
            Entity entity = this.EntityManager.CreateEntity();
            TestSharedComponent sharedComponent = new TestSharedComponent() {
                value = 1 // test value here to compare later
            };
            this.EntityManager.AddSharedComponentData(entity, sharedComponent);

            // Act
            TestSystem system = this.World.CreateSystem<TestSystem>();
            system.Update();
            
            
            // Assert
            Assert.IsTrue(sharedComponent.value == system.ComponentFromQuery.value); 
        }
    }
}
