using NUnit.Framework;

namespace GoapBrainEcs.Tests {
    [TestFixture]
    [Category("GoapBrainEcs")]
    public class ConditionHashSetTest {
        [Test]
        public void TestAdd() {
            ConditionHashSet hashSet = new ConditionHashSet();
            hashSet.Add(new Condition(2, true));
            Assert.True(hashSet.Count == 1);
            Assert.True(hashSet.Contains(new Condition(2, true)));
            
            hashSet.Add(new Condition(5, false));
            Assert.True(hashSet.Count == 2);
            Assert.True(hashSet.Contains(new Condition(5, false)));
            
            hashSet.Add(new Condition(7, true));
            Assert.True(hashSet.Count == 3);
            Assert.True(hashSet.Contains(new Condition(7, true)));
            
            // Should not contain condition with different value
            Assert.False(hashSet.Contains(new Condition(7, false)));
        }

        [Test]
        public void TestDuplicateAdd() {
            ConditionHashSet hashSet = new ConditionHashSet();
            hashSet.Add(new Condition(2, true));
            hashSet.Add(new Condition(5, false));
            hashSet.Add(new Condition(7, true));
            
            // Should not add as it already contains such item
            hashSet.Add(7, true);
            Assert.IsTrue(hashSet.Count == 3);
            
            hashSet.Add(2, true);
            Assert.IsTrue(hashSet.Count == 3);
            
            hashSet.Add(5, false);
            Assert.IsTrue(hashSet.Count == 3);
            
            // It's a new item, it should be added
            hashSet.Add(11, true);
            Assert.IsTrue(hashSet.Count == 4);
        }
        
        [Test]
        public void TestRemove() {
            ConditionHashSet hashSet = new ConditionHashSet();
            hashSet.Add(new Condition(2, true));
            hashSet.Add(new Condition(5, false));
            hashSet.Add(new Condition(7, true));
            
            hashSet.Remove(5, false);
            Assert.IsFalse(hashSet.Contains(new Condition(5, false)));
            Assert.IsTrue(hashSet.Count == 2);
            
            // Remove an item not in the hash set
            hashSet.Remove(5, true);
            Assert.IsTrue(hashSet.Count == 2);
            
            hashSet.Remove(7, true);
            Assert.IsFalse(hashSet.Contains(new Condition(7, true)));
            Assert.IsTrue(hashSet.Count == 1);
            
            hashSet.Remove(2, true);
            Assert.IsFalse(hashSet.Contains(new Condition(2, true)));
            Assert.IsTrue(hashSet.Count == 0);
        }
        
        [Test]
        public void TestClear() {
            ConditionHashSet hashSet = new ConditionHashSet();
            hashSet.Add(new Condition(2, true));
            hashSet.Add(new Condition(5, false));
            hashSet.Add(new Condition(7, true));
            
            hashSet.Clear();
            Assert.IsTrue(hashSet.Count == 0);
            
            hashSet.Add(10, false);
            Assert.IsTrue(hashSet.Count == 1);
            Assert.IsTrue(hashSet.Contains(new Condition(10, false)));
        }
    }
}