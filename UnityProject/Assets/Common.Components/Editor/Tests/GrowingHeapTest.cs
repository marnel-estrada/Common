using NUnit.Framework;

namespace Common.Test {
    /// <summary>
    /// This is a heap implementation derived from https://forum.unity.com/threads/priority-queue-min-heap-implementation-with-native-containers.569161/
    /// </summary>
    [TestFixture]
    [Category("Common")]
    public class GrowingHeapTest {
        private struct Item {
            public readonly int value;

            public Item(int value) {
                this.value = value;
            }
        }
        
        [Test]
        public void TestPush() {
            GrowingHeap<Item> heap = new GrowingHeap<Item>(10);
            heap.Push(new Item(0), 0);
            Assert.True(heap.Count == 1);
            
            heap.Push(new Item(1), 1);
            Assert.True(heap.Count == 2);
        }
        
        [Test]
        public void TestCount() {
            GrowingHeap<Item> heap = new GrowingHeap<Item>(10);
            heap.Push(new Item(0), 0);
            Assert.True(heap.Count == 1);
            
            heap.Push(new Item(1), 1);
            Assert.True(heap.Count == 2);
            
            heap.Push(new Item(2), 2);
            heap.Push(new Item(3), 3);
            heap.Push(new Item(4), 4);
            heap.Push(new Item(5), 5);
            Assert.True(heap.Count == 6);

            heap.Pop();
            Assert.True(heap.Count == 5);
            
            heap.Pop();
            Assert.True(heap.Count == 4);
            
            heap.Pop();
            heap.Pop();
            Assert.True(heap.Count == 2);
        }
        
        [Test]
        public void TestPop() {
            GrowingHeap<Item> heap = new GrowingHeap<Item>(10);
            heap.Push(new Item(0), 0);
            heap.Push(new Item(1), 1);
            heap.Push(new Item(2), 2);
            heap.Push(new Item(5), 5);
            heap.Push(new Item(4), 4);
            heap.Push(new Item(3), 3);

            float previousCost = int.MinValue;
            while (heap.HasItems) {
                Item top = heap.Top;
                Assert.True(previousCost < top.value);
                previousCost = top.value;
                heap.Pop();
            }
        }
        
        [Test]
        public void TestHasItems() {
            GrowingHeap<Item> heap = new GrowingHeap<Item>(10);
            Assert.False(heap.HasItems);
            
            heap.Push(new Item(0), 0);
            Assert.True(heap.HasItems);
            
            heap.Push(new Item(1), 1);
            heap.Push(new Item(2), 2);
            heap.Push(new Item(3), 3);
            Assert.True(heap.HasItems);

            while (heap.HasItems) {
                heap.Pop();
            }
            Assert.False(heap.HasItems);
            
            heap.Push(new Item(4), 4);
            Assert.True(heap.HasItems);
            
            heap.Clear();
            Assert.False(heap.HasItems);
        }
        
        [Test]
        public void TestClear() {
            GrowingHeap<Item> heap = new GrowingHeap<Item>(10);

            const int COUNT = 1000;
            for (int i = 0; i < COUNT; ++i) {
                heap.Push(new Item(i), i);
            }
            Assert.True(heap.Count == COUNT);
            
            heap.Clear();
            Assert.True(heap.Count == 0);
            Assert.False(heap.HasItems);
        }
        
        // Test preventing infinite loop while pushing
        [Test]
        public void TestNextNotPointingToItself() {
            GrowingHeap<Item> heap = new GrowingHeap<Item>(10);
            
            // The values here are derived from a bug that happened during A* search
            heap.Push(new Item(24), 24);
            heap.Pop();
            heap.Push(new Item(24), 15);
            heap.Push(new Item(24), 35);
            heap.Push(new Item(24), 29);
            heap.Push(new Item(24), 21);
            Assert.True(heap.CheckIntegrity());
            heap.Pop();
            Assert.True(heap.CheckIntegrity());
            heap.Push(new Item(24), 12);
            Assert.True(heap.CheckIntegrity());
            heap.Push(new Item(24), 26);
            Assert.True(heap.CheckIntegrity());
            heap.Push(new Item(24), 12);
            Assert.True(heap.CheckIntegrity());
            heap.Pop();
            Assert.True(heap.CheckIntegrity());
        }
    }
}