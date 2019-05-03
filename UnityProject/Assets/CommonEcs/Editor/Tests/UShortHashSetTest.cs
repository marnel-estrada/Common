using NUnit.Framework;

namespace CommonEcs {
    [TestFixture]
    [Category("CommonEcs")]
    public class UShortHashSetTest {
        [Test]
        public void TestAdd() {
            UShortHashSet hashSet = new UShortHashSet();
            hashSet.Add(2);
            Assert.True(hashSet.Count == 1);
            Assert.True(hashSet.Contains(2));
            
            hashSet.Add(5);
            Assert.True(hashSet.Count == 2);
            Assert.True(hashSet.Contains(5));
            
            hashSet.Add(7);
            Assert.True(hashSet.Count == 3);
            Assert.True(hashSet.Contains(7));
        }

        [Test]
        public void TestDuplicateAdd() {
            UShortHashSet hashSet = new UShortHashSet();
            hashSet.Add(2);
            hashSet.Add(5);
            hashSet.Add(7);
            
            // Should not add as it already contains such item
            hashSet.Add(7);
            Assert.IsTrue(hashSet.Count == 3);
            
            hashSet.Add(2);
            Assert.IsTrue(hashSet.Count == 3);
            
            hashSet.Add(5);
            Assert.IsTrue(hashSet.Count == 3);
            
            // It's a new item, it should be added
            hashSet.Add(11);
            Assert.IsTrue(hashSet.Count == 4);
        }

        [Test]
        public void TestRemove() {
            UShortHashSet hashSet = new UShortHashSet();
            hashSet.Add(2);
            hashSet.Add(5);
            hashSet.Add(7);
            
            hashSet.Remove(5);
            Assert.IsTrue(hashSet.Count == 2);
            
            // Remove an item not in the hash set
            hashSet.Remove(888);
            Assert.IsTrue(hashSet.Count == 2);
            
            hashSet.Remove(7);
            Assert.IsTrue(hashSet.Count == 1);
            
            hashSet.Remove(2);
            Assert.IsTrue(hashSet.Count == 0);
        }

        [Test]
        public void TestClear() {
            UShortHashSet hashSet = new UShortHashSet();
            hashSet.Add(2);
            hashSet.Add(5);
            hashSet.Add(7);
            
            hashSet.Clear();
            Assert.IsTrue(hashSet.Count == 0);
            
            hashSet.Add(10);
            Assert.IsTrue(hashSet.Count == 1);
            Assert.IsTrue(hashSet.Contains(10));
        }
    }
}