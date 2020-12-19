using CommonEcs;

using NUnit.Framework;

using Unity.Entities;
using Unity.Entities.Tests;
using Unity.Jobs;

namespace GoapBrainEcs {
    [TestFixture]
    [Category("GoapBrainEcs")]
    public class SampleEcsTest : ECSTestsFixture {
        [Test]
        public void SimpleTest() {
            this.EntityManager.World.GetOrCreateSystem<IncrementCounterSystem>().Update();
            Assert.Pass();
        }

        [Test]
        public void TestBufferElement() {
            Entity entity = this.EntityManager.CreateEntity(typeof(BufferElement<Entity>));
            DynamicBuffer<BufferElement<Entity>> targets = this.EntityManager.GetBuffer<BufferElement<Entity>>(entity);
            Assert.Pass();
        }

        private class SplineSystem : JobSystemBase {
            private EntityQuery query;
        
            protected override void OnCreate() {
                this.query = GetEntityQuery(typeof(BufferElement<Counter>));
            }
        
            protected override JobHandle OnUpdate(JobHandle inputDeps) {
                Job job = new Job() {
                    splinePointType = GetBufferTypeHandle<BufferElement<Counter>>()
                };
        
                return JobChunkExtensions.Schedule(job, this.query, inputDeps);
            }
        }

        private struct Job : IJobChunk {
            public BufferTypeHandle<BufferElement<Counter>> splinePointType;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                BufferAccessor<BufferElement<Counter>> pointsList = chunk.GetBufferAccessor(this.splinePointType);
                for (int i = 0; i < pointsList.Length; ++i) {
                    DynamicBuffer<BufferElement<Counter>> points = pointsList[i];
                    // Do something with points here
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