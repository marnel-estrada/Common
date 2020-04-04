using NUnit.Framework;

using Unity.Entities;

using Assert = UnityEngine.Assertions.Assert;

namespace CommonEcs {
    [TestFixture]
    [Category("CommonEcs")]
    public class EntityReferenceTest : CommonEcsTest {
        [Test]
        public void TestBasic() {
            Entity a = this.EntityManager.CreateEntity();
            Entity b = this.EntityManager.CreateEntity();
            
            // a references b
            EntityReference.Create(a, b, this.EntityManager);
            
            this.EntityManager.DestroyEntity(a);
            this.World.GetOrCreateSystem<DestroyUnownedEntityReferencesSystem>().Update();
            this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().Update(); // We invoke the barrier to flush the EntityCommandBuffer
            
            // b should be destroyed after DestroyUnownedEntityReferencesSystem executes
            Assert.IsFalse(this.EntityManager.Exists(b));
        }

        [Test]
        public void TestReferenceCycle() {
            Entity a = this.EntityManager.CreateEntity();
            Entity b = this.EntityManager.CreateEntity();
            
            EntityReference.Create(a, b, this.EntityManager);
            EntityReference.Create(b, a, this.EntityManager);
            
            this.EntityManager.DestroyEntity(a);
            this.World.GetOrCreateSystem<DestroyUnownedEntityReferencesSystem>().Update();
            this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().Update();
            
            // Both should no longer exist when one of them is destroyed since the other has nothing 
            // pointing to it
            Assert.IsFalse(this.EntityManager.Exists(a));
            Assert.IsFalse(this.EntityManager.Exists(b));
        }
        
        // EntityReference does not support multiple reference yet due to it component nature
        // An entity can't have multiple components
        //[Test]
        public void TestMultipleOwners() {
            Entity a = this.EntityManager.CreateEntity();
            Entity b = this.EntityManager.CreateEntity();
            Entity c = this.EntityManager.CreateEntity();
            
            // c is both referenced by a and b
            EntityReference.Create(a, c, this.EntityManager);
            EntityReference.Create(b, c, this.EntityManager);
            
            this.EntityManager.DestroyEntity(a);
            this.World.GetOrCreateSystem<DestroyUnownedEntityReferencesSystem>().Update();
            this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().Update();
            
            // c should still exist because b is still referencing it
            Assert.IsTrue(this.EntityManager.Exists(c));
            
            this.EntityManager.DestroyEntity(b);
            this.World.GetOrCreateSystem<DestroyUnownedEntityReferencesSystem>().Update();
            this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().Update();
            
            // At this point, c should also be removed as well
            Assert.IsFalse(this.EntityManager.Exists(c));
        }

        // EntityReference does not support multiple reference yet due to it component nature
        // An entity can't have multiple components
        //[Test]
        public void TestReferenceCycleButOneIsReferencedByAnother() {
            Entity a = this.EntityManager.CreateEntity();
            Entity b = this.EntityManager.CreateEntity();
            Entity c = this.EntityManager.CreateEntity();
            
            EntityReference.Create(a, b, this.EntityManager);
            EntityReference.Create(b, a, this.EntityManager);
            EntityReference.Create(c, b, this.EntityManager);
            
            this.EntityManager.DestroyEntity(a);
            this.World.GetOrCreateSystem<DestroyUnownedEntityReferencesSystem>().Update();
            this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().Update();
            
            // a should no longer exist but b should remain as it is still referenced from c
            Assert.IsFalse(this.EntityManager.Exists(a));
            Assert.IsTrue(this.EntityManager.Exists(b));
            
            this.EntityManager.DestroyEntity(c);
            this.World.GetOrCreateSystem<DestroyUnownedEntityReferencesSystem>().Update();
            this.World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().Update();
            
            // But when c is destroyed, b should have been destroyed as well
            Assert.IsFalse(this.EntityManager.Exists(b));
        }
    }
}