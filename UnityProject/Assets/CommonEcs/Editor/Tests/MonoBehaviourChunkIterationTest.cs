using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;

using UnityEngine;

namespace CommonEcs.Test {
    [TestFixture]
    [Category("CommonEcs")]
    public class MonoBehaviourChunkIterationTest : ECSTestsFixture {
        [Test]
        public void TestMonoBehaviourChunkIteration() {
            const int goCount = 5;
            for (int i = 0; i < goCount; ++i) {
                GameObject go = new GameObject("Test" + i);
                go.transform.position = new Vector3(i * 2, i * 2, i * 2);
                go.AddComponent<GameObjectEntity>();
            }

            TestSystem system = this.World.CreateSystem<TestSystem>();
            system.Update();
        }
        
        private class TestSystem : ComponentSystem {
            private EntityQuery query;
            private ArchetypeChunkComponentType<Transform> transformType;
            
            protected override void OnCreateManager() {
                this.query = GetEntityQuery(typeof(Transform));
                this.Enabled = false;
            }

            protected override void OnUpdate() {
                this.transformType = GetArchetypeChunkComponentType<Transform>();
                
                NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
                for (int i = 0; i < chunks.Length; ++i) {
                    Process(chunks[i]);
                }
                
                chunks.Dispose();
            }

            private void Process(ArchetypeChunk chunk) {
                ArchetypeChunkComponentObjects<Transform> transforms = chunk.GetComponentObjects(this.transformType, this.EntityManager);
                for (int i = 0; i < transforms.Length; ++i) {
                    Transform transform = transforms[i];
                    Debug.Log($"{i}: {transform.position.x}");
                }
            }
        }
    }
}