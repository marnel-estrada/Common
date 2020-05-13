using NUnit.Framework;

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
        
        private class TestSystem : SystemBase {
            protected override void OnCreate() {
                this.Enabled = false;
            }

            protected override void OnUpdate() {
                this.Entities.ForEach(delegate(Transform transform) {
                    Debug.Log($"{transform.gameObject.name}: {transform.position.x.ToString()}");
                }).WithoutBurst().Run();
            }
        }
    }
}